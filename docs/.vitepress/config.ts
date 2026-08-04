import fs from 'node:fs'
import path from 'node:path'
import { defineConfig, type SiteConfig } from 'vitepress'

const repo = 'https://github.com/Sonberg/zeta'
const site = 'https://sonberg.github.io/zeta'

// Single source of truth: drives both the rendered sidebar and the generated
// llms.txt, so the two can never drift apart.
const sidebar = [
  {
    text: 'Introduction',
    items: [
      { text: 'What is Zeta?', link: '/' },
      { text: 'Getting started', link: '/getting-started' }
    ]
  },
  {
    text: 'Core concepts',
    items: [
      { text: 'Schema types', link: '/schemas' },
      { text: 'Validator reference', link: '/validators' },
      { text: 'Results and errors', link: '/results' },
      { text: 'Validation paths', link: '/paths' }
    ]
  },
  {
    text: 'Building schemas',
    items: [
      { text: 'Fluent property builders', link: '/property-builders' },
      { text: 'Collections', link: '/collections' },
      { text: 'Dictionaries', link: '/dictionaries' },
      { text: 'Conditionals and polymorphism', link: '/conditionals' },
      { text: 'Custom rules', link: '/custom-rules' },
      { text: 'Context-aware validation', link: '/validation-run' }
    ]
  },
  {
    text: 'Integrations',
    items: [
      { text: 'ASP.NET Core', link: '/aspnetcore' },
      { text: 'FastEndpoints', link: '/fastendpoints' },
      { text: 'Mediator', link: '/mediator' }
    ]
  },
  {
    text: 'Practices',
    items: [
      { text: 'Testing', link: '/testing' },
      { text: 'Glossary', link: '/glossary' },
      { text: 'Changelog', link: '/changelog' }
    ]
  }
]

/** Resolve `<!--@include: path-->` directives the way VitePress does. */
function resolveIncludes(content: string, filePath: string): string {
  return content.replace(/^<!--\s*@include:\s*(.+?)\s*-->\s*$/gm, (whole, target: string) => {
    const resolved = path.resolve(path.dirname(filePath), target.replace(/\{.*\}$/, '').trim())

    try {
      return fs.readFileSync(resolved, 'utf-8')
    } catch {
      return whole
    }
  })
}

/** Split a page into its frontmatter fields and its markdown body. */
function splitFrontmatter(content: string): { data: Record<string, string>; body: string } {
  const match = content.match(/^---\r?\n([\s\S]*?)\r?\n---\r?\n/)

  if (!match) return { data: {}, body: content }

  const data: Record<string, string> = {}

  for (const line of match[1].split(/\r?\n/)) {
    const field = line.match(/^([A-Za-z_][\w-]*):\s*(.+)$/)

    if (field) data[field[1]] = field[2].trim().replace(/^["']|["']$/g, '')
  }

  return { data, body: content.slice(match[0].length) }
}

// Periods that end an abbreviation rather than a sentence.
const abbreviations = /(?:^|[\s([{"'])(?:e\.g|i\.e|etc|vs|cf|approx|Dr|Mr|Ms|No)$/i

/** First sentence of the first real paragraph — the llms.txt link description. */
function summarise(markdown: string): string {
  const lines = markdown.split(/\r?\n/)
  const paragraph: string[] = []
  let inFence = false

  const flush = (): string | null => {
    if (!paragraph.length) return null

    const text = paragraph
      .join(' ')
      .replace(/!\[[^\]]*\]\([^)]*\)/g, '')
      .replace(/\[([^\]]+)\]\([^)]*\)/g, '$1')
      .replace(/[`*_]/g, '')
      .replace(/\s+/g, ' ')
      .trim()

    paragraph.length = 0

    return text.length >= 20 ? text : null
  }

  for (const rawLine of lines) {
    const line = rawLine.trim()

    if (line.startsWith('```')) {
      inFence = !inFence
      paragraph.length = 0
      continue
    }

    if (inFence) continue

    // Blank line ends a paragraph; take it if it is substantial enough.
    if (!line) {
      const text = flush()
      if (text) return firstSentence(text)
      continue
    }

    // Headings, tables, lists, quotes, directives and HTML are not prose.
    if (/^(#|\||-|\*|>|:::|<)/.test(line)) {
      paragraph.length = 0
      continue
    }

    paragraph.push(line)
  }

  const text = flush()

  return text ? firstSentence(text) : ''
}

function firstSentence(text: string): string {
  let cursor = 0

  while (cursor < text.length) {
    const period = text.indexOf('. ', cursor)

    if (period === -1) break

    if (!abbreviations.test(text.slice(0, period))) {
      return text.slice(0, period)
    }

    cursor = period + 2
  }

  return text.endsWith('.') ? text.slice(0, -1) : text
}

function pageFile(srcDir: string, link: string): string {
  return path.join(srcDir, link === '/' ? 'index.md' : `${link.replace(/^\//, '')}.md`)
}

function readPage(srcDir: string, link: string): { body: string; description: string } | null {
  const file = pageFile(srcDir, link)

  if (!fs.existsSync(file)) return null

  const { data, body } = splitFrontmatter(fs.readFileSync(file, 'utf-8'))
  const resolved = resolveIncludes(body, file)

  // An explicit frontmatter description always wins over extraction.
  return { body: resolved, description: data.description || summarise(resolved) }
}

/**
 * Emit llms.txt (an index, per llmstxt.org) and llms-full.txt (every page
 * inlined) into the built site. Generated from the sidebar rather than
 * hand-written so it stays correct as pages are added or renamed.
 */
function writeLlmsTxt({ srcDir, outDir }: SiteConfig) {
  const index: string[] = [
    '# Zeta',
    '',
    '> A composable, type-safe, async-first validation framework for .NET, inspired by Zod. Schemas are immutable values built from the `Z` entry point; validation is async by default and returns a `Result<T>` rather than throwing, with JSONPath-aware errors carrying a stable machine-readable code.',
    '',
    'Requires .NET 6 or later. The `Zeta.AspNetCore` and `Zeta.FastEndpoints` integration packages require .NET 8 or later.',
    '',
    'Note on naming: `Z.Object<T>()` and `.Field(...)` were removed in 0.1.17 (they are not aliases), and the execution record `ValidationContext` was renamed to `ValidationRun`. Code using the old names will not compile.',
    ''
  ]

  const full: string[] = [
    '# Zeta — full documentation',
    '',
    `Generated from ${site}. Every documentation page, inlined.`,
    ''
  ]

  for (const group of sidebar) {
    index.push(`## ${group.text}`, '')

    for (const item of group.items) {
      const page = readPage(srcDir, item.link)

      if (page === null) continue

      const url = `${site}${item.link === '/' ? '/' : item.link}`

      index.push(`- [${item.text}](${url})${page.description ? `: ${page.description}` : ''}`)

      full.push(`# ${item.text}`, '', `Source: ${url}`, '', page.body.trim(), '', '---', '')
    }

    index.push('')
  }

  index.push(
    '## Optional',
    '',
    `- [Full documentation as one file](${site}/llms-full.txt): Every page above, inlined`,
    `- [Repository](${repo}): Source, samples, benchmarks and tests`,
    `- [Changelog](${repo}/blob/main/CHANGELOG.md): Release history`,
    ''
  )

  fs.writeFileSync(path.join(outDir, 'llms.txt'), index.join('\n'), 'utf-8')
  fs.writeFileSync(path.join(outDir, 'llms-full.txt'), full.join('\n'), 'utf-8')
}

export default defineConfig({
  title: 'Zeta',
  description: 'A composable, type-safe, async-first validation framework for .NET.',
  lang: 'en-US',

  // Published at https://sonberg.github.io/zeta/
  base: '/zeta/',
  cleanUrls: true,
  lastUpdated: true,

  head: [
    ['meta', { name: 'theme-color', content: '#7c5cff' }],
    ['meta', { property: 'og:type', content: 'website' }],
    ['meta', { property: 'og:title', content: 'Zeta — validation for .NET' }],
    [
      'meta',
      {
        property: 'og:description',
        content: 'Schema-first, async-first validation for .NET. Composable schemas, a Result pattern instead of exceptions, and path-aware errors.'
      }
    ],
    ['link', { rel: 'alternate', type: 'text/plain', href: '/zeta/llms.txt', title: 'llms.txt' }]
  ],

  themeConfig: {
    siteTitle: 'Zeta',

    search: {
      provider: 'local'
    },

    nav: [
      { text: 'Guide', link: '/getting-started' },
      { text: 'Validators', link: '/validators' },
      { text: 'Integrations', link: '/aspnetcore' },
      {
        text: 'Resources',
        items: [
          { text: 'Changelog', link: '/changelog' },
          { text: 'Glossary', link: '/glossary' },
          { text: 'llms.txt', link: `${site}/llms.txt` },
          { text: 'Zeta on NuGet', link: 'https://www.nuget.org/packages/Zeta' },
          { text: 'Contributing', link: `${repo}/blob/main/CONTRIBUTING.md` }
        ]
      }
    ],

    sidebar,

    socialLinks: [{ icon: 'github', link: repo }],

    editLink: {
      pattern: `${repo}/edit/main/docs/:path`,
      text: 'Edit this page on GitHub'
    },

    footer: {
      message: 'Released under the MIT License.',
      copyright: `Copyright © ${new Date().getFullYear()} Per Sonberg`
    },

    outline: [2, 3],

    docFooter: {
      prev: 'Previous',
      next: 'Next'
    }
  },

  markdown: {
    theme: {
      light: 'github-light',
      dark: 'github-dark'
    },
    lineNumbers: false
  },

  sitemap: {
    hostname: 'https://sonberg.github.io/zeta/'
  },

  buildEnd: writeLlmsTxt
})
