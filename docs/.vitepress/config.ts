import { defineConfig } from 'vitepress'

const repo = 'https://github.com/Sonberg/zeta'

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
    ]
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
          { text: 'Zeta on NuGet', link: 'https://www.nuget.org/packages/Zeta' },
          { text: 'Contributing', link: `${repo}/blob/main/CONTRIBUTING.md` }
        ]
      }
    ],

    sidebar: [
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
    ],

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
  }
})
