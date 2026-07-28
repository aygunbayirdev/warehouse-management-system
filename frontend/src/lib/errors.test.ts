import { AxiosError, AxiosHeaders } from 'axios'
import { describe, expect, it } from 'vitest'

import { getApiErrorMessage } from './errors'

function problemDetailsError(title: string): AxiosError {
  return Object.assign(new AxiosError('Request failed'), {
    response: {
      status: 409,
      data: { title },
      statusText: 'Conflict',
      headers: {},
      config: { headers: new AxiosHeaders() },
    },
  })
}

describe('getApiErrorMessage', () => {
  it('maps a known error title to a Turkish message', () => {
    expect(getApiErrorMessage(problemDetailsError('Category.InUse'))).toBe(
      'Bu kategori en az bir üründe kullanılıyor, silinemez.',
    )
  })

  it('returns the fallback for an unknown error title', () => {
    expect(
      getApiErrorMessage(problemDetailsError('Something.Unexpected')),
    ).toBe('Bir hata oluştu. Lütfen tekrar deneyin.')
  })

  it('returns a custom fallback when provided', () => {
    expect(
      getApiErrorMessage(problemDetailsError('Something.Unexpected'), 'Özel mesaj'),
    ).toBe('Özel mesaj')
  })

  it('returns the fallback for non-Axios errors', () => {
    expect(getApiErrorMessage(new Error('boom'))).toBe(
      'Bir hata oluştu. Lütfen tekrar deneyin.',
    )
  })
})
