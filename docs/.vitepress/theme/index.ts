import { h } from 'vue'
import DefaultTheme from 'vitepress/theme'
import type { Theme } from 'vitepress'
import MigrateWithAI from './MigrateWithAI.vue'

export default {
  extends: DefaultTheme,
  Layout: () =>
    h(DefaultTheme.Layout, null, {
      'nav-bar-content-after': () => h(MigrateWithAI, { compact: true }),
      'home-hero-actions-after': () => h(MigrateWithAI)
    }),
  enhanceApp({ app }) {
    app.component('MigrateWithAI', MigrateWithAI)
  }
} satisfies Theme
