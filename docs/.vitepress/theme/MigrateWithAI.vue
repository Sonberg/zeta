<script setup lang="ts">
import { ref } from 'vue'
import { data as prompt } from './migrate-prompt.data'

defineProps<{ compact?: boolean }>()

type State = 'idle' | 'copied' | 'failed'

const state = ref<State>('idle')

async function copy() {
  try {
    await navigator.clipboard.writeText(prompt)
    state.value = 'copied'
  } catch {
    state.value = 'failed'
  }

  setTimeout(() => (state.value = 'idle'), 2000)
}

const labels: Record<State, string> = {
  idle: 'Migrate with AI',
  copied: 'Copied!',
  failed: 'Copy failed'
}
</script>

<template>
  <button
    type="button"
    class="migrate-with-ai"
    :class="{ 'migrate-with-ai--compact': compact, 'migrate-with-ai--brand': !compact }"
    :title="'Copy a migration prompt with the full Zeta docs, ready to paste into an AI assistant'"
    @click="copy"
  >
    {{ compact ? (state === 'idle' ? 'Migrate with AI' : labels[state]) : labels[state] }}
  </button>
</template>

<style scoped>
.migrate-with-ai {
  border-radius: 20px;
  font-weight: 600;
  white-space: nowrap;
  transition:
    color 0.25s,
    border-color 0.25s,
    background-color 0.25s;
  cursor: pointer;
}

.migrate-with-ai--brand {
  display: inline-block;
  border: 1px solid var(--vp-button-alt-border);
  padding: 0 20px;
  line-height: 38px;
  font-size: 14px;
  color: var(--vp-button-alt-text);
  background-color: var(--vp-button-alt-bg);
}

.migrate-with-ai--brand:hover {
  border-color: var(--vp-button-alt-hover-border);
  color: var(--vp-button-alt-hover-text);
  background-color: var(--vp-button-alt-hover-bg);
}

.migrate-with-ai--compact {
  display: none;
  border: 1px solid var(--vp-c-brand-1);
  padding: 0 12px;
  line-height: 30px;
  font-size: 13px;
  color: var(--vp-c-brand-1);
  background-color: transparent;
}

.migrate-with-ai--compact:hover {
  color: var(--vp-c-white);
  background-color: var(--vp-c-brand-1);
}

@media (min-width: 960px) {
  .migrate-with-ai--compact {
    display: inline-block;
  }
}
</style>
