<script setup lang="ts">
import { VPButton } from 'vitepress/theme'
import MigrateWithAI from './MigrateWithAI.vue'
</script>

<template>
  <div class="hero-actions">
    <VPButton tag="a" theme="brand" size="medium" text="Get started" href="/getting-started" />
    <MigrateWithAI />
  </div>
</template>

<style scoped>
.hero-actions {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
  padding-top: 24px;
}

@media (min-width: 640px) {
  .hero-actions {
    padding-top: 32px;
  }
}
</style>
