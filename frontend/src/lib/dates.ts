export function formatUtcDateTime(value: string): string {
  const iso = value.endsWith('Z') ? value : `${value}Z`
  return new Date(iso).toLocaleString('tr-TR')
}
