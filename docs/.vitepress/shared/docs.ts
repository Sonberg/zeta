import fs from 'node:fs'
import path from 'node:path'

// Single source of truth: drives the rendered sidebar, the generated
// llms.txt/llms-full.txt, and the "Migrate with AI" prompt, so none of them
// can drift apart.
export const sidebar = [
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
export function resolveIncludes(content: string, filePath: string): string {
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
export function splitFrontmatter(content: string): { data: Record<string, string>; body: string } {
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
export function summarise(markdown: string): string {
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

export function firstSentence(text: string): string {
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

export function pageFile(srcDir: string, link: string): string {
  return path.join(srcDir, link === '/' ? 'index.md' : `${link.replace(/^\//, '')}.md`)
}

export function readPage(srcDir: string, link: string): { body: string; description: string } | null {
  const file = pageFile(srcDir, link)

  if (!fs.existsSync(file)) return null

  const { data, body } = splitFrontmatter(fs.readFileSync(file, 'utf-8'))
  const resolved = resolveIncludes(body, file)

  // An explicit frontmatter description always wins over extraction.
  return { body: resolved, description: data.description || summarise(resolved) }
}
