import { defineLoader } from 'vitepress'
import { sidebar, readPage } from '../shared/docs'

declare const data: string
export { data }

const preamble = `I want to migrate an existing .NET project to use Zeta, a composable, type-safe, async-first validation framework (https://github.com/Sonberg/zeta). Please:

1. Find where validation currently happens in my project (DataAnnotations, FluentValidation, manual if-checks, etc.) — ask me for the relevant files (DTOs/commands, validators, controllers or endpoints) if you don't already have them.
2. Map each validated type to a Zeta schema built from the \`Z\` entry point (\`Z.Schema<T>()\`, \`.Property(...)\`, built-in validators like \`.Email()\`/\`.Min()\`/\`.MaxLength()\`).
3. For any rule that needs async data (uniqueness checks, DB/HTTP lookups), promote the schema with \`.Using<TContext>(factory)\` and use \`.Refine\`/\`.RefineAsync\` against the context instead of injecting services directly into the rule.
4. If the project is ASP.NET Core or FastEndpoints, wire up the matching integration package (\`Zeta.AspNetCore\` or \`Zeta.FastEndpoints\`) so schemas run as part of the request pipeline.
5. Update or write tests that assert on \`Result<T>\` (\`IsSuccess\`, \`Errors\`, \`error.Code\`/\`error.PathString\`) instead of expecting thrown exceptions.

Below is the complete Zeta documentation for reference.

---
`

export default defineLoader({
  watch: ['../../*.md'],
  load(): string {
    const srcDir = new URL('../../', import.meta.url).pathname
    const full: string[] = []

    for (const group of sidebar) {
      for (const item of group.items) {
        const page = readPage(srcDir, item.link)

        if (page === null) continue

        full.push(`# ${item.text}`, '', page.body.trim(), '', '---', '')
      }
    }

    return preamble + full.join('\n')
  }
})
