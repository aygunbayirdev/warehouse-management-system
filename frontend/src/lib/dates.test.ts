import { describe, expect, it } from 'vitest'

import { formatUtcDateTime } from './dates'

describe('formatUtcDateTime', () => {
  it('interprets a Z-less DateTime string as UTC, not local time', () => {
    const withoutZ = formatUtcDateTime('2026-01-01T00:00:00')
    const withZ = new Date('2026-01-01T00:00:00Z').toLocaleString('tr-TR')

    expect(withoutZ).toBe(withZ)
  })

  it('leaves a string that already ends with Z unchanged before parsing', () => {
    const withZ = formatUtcDateTime('2026-01-01T00:00:00Z')
    expect(withZ).toBe(new Date('2026-01-01T00:00:00Z').toLocaleString('tr-TR'))
  })
})
