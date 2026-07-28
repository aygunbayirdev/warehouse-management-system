import { AxiosError } from 'axios'

const KNOWN_ERROR_MESSAGES: Record<string, string> = {
  'Category.InUse': 'Bu kategori en az bir üründe kullanılıyor, silinemez.',
  'UnitOfMeasure.InUse': 'Bu birim en az bir üründe kullanılıyor, silinemez.',
  'Warehouse.InUse': 'Bu deponun stok kayıtları var, silinemez.',
  'Auth.InvalidCredentials': 'E-posta veya şifre hatalı.',
}

const DEFAULT_FALLBACK = 'Bir hata oluştu. Lütfen tekrar deneyin.'

export function getApiErrorMessage(
  error: unknown,
  fallback: string = DEFAULT_FALLBACK,
): string {
  if (error instanceof AxiosError) {
    const title = (error.response?.data as { title?: string } | undefined)
      ?.title
    if (title && KNOWN_ERROR_MESSAGES[title]) {
      return KNOWN_ERROR_MESSAGES[title]
    }
  }

  return fallback
}
