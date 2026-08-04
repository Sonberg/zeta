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
    <svg
      v-if="state === 'copied'"
      class="migrate-with-ai__icon"
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2.5"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <polyline points="20 6 9 17 4 12" />
    </svg>
    <svg
      v-else
      class="migrate-with-ai__icon"
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      stroke-width="2"
      stroke-linecap="round"
      stroke-linejoin="round"
    >
      <rect x="8" y="2" width="8" height="4" rx="1" ry="1" />
      <path d="M16 4h2a2 2 0 0 1 2 2v14a2 2 0 0 1-2 2H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h2" />
    </svg>
    <span>{{ compact ? (state === 'idle' ? 'Migrate with AI' : labels[state]) : labels[state] }}</span>
  </button>
</template>

<style scoped>
.migrate-with-ai {
  border-radius: 20px;
  font-weight: 600;
  white-space: nowrap;
  align-items: center;
  gap: 6px;
  transition:
    color 0.25s,
    border-color 0.25s,
    background-color 0.25s;
  cursor: pointer;
}

.migrate-with-ai__icon {
  flex-shrink: 0;
}

.migrate-with-ai--brand {
  display: inline-flex;
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
    display: inline-flex;
  }
}
</style>
