import fs from 'node:fs'
import path from 'node:path'
import { defineConfig, type SiteConfig } from 'vitepress'
import { sidebar, readPage } from './shared/docs'

const repo = 'https://github.com/Sonberg/zeta'
const site = 'https://sonberg.github.io/zeta'

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
