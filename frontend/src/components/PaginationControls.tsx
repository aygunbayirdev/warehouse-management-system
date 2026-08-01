import { Button } from '@/components/ui/button'

type PaginationControlsProps = {
  page: number
  totalPages: number
  totalCount: number
  pageSize: number
  onPageChange: (page: number) => void
}

export function PaginationControls({
  page,
  totalPages,
  totalCount,
  pageSize,
  onPageChange,
}: PaginationControlsProps) {
  return (
    <div className="flex items-center justify-between text-sm text-muted-foreground">
      <span>
        {totalCount === 0
          ? 'Kayıt yok'
          : `${(page - 1) * pageSize + 1}–${Math.min(page * pageSize, totalCount)} / ${totalCount}`}
      </span>
      <div className="flex items-center gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Önceki
        </Button>
        <span>
          Sayfa {page} / {totalPages}
        </span>
        <Button
          variant="outline"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Sonraki
        </Button>
      </div>
    </div>
  )
}
