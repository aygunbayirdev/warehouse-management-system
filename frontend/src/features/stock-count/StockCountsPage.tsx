import { Plus } from 'lucide-react'
import { type FormEvent, useMemo, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { toast } from 'sonner'

import { PaginationControls } from '@/components/PaginationControls'
import { Button } from '@/components/ui/button'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogTitle,
} from '@/components/ui/dialog'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { RoleNames } from '@/features/auth/types'
import { useWarehouses } from '@/features/warehouses/api/warehouses'
import { formatUtcDateTime } from '@/lib/dates'
import { getApiErrorMessage } from '@/lib/errors'
import { cn } from '@/lib/utils'

import { useCreateStockCount, useStockCounts } from './api/stockCounts'
import type { StockCountStatus } from './types'

const ALL_VALUE = 'all'
const SESSION_MANAGE_ROLES = [
  RoleNames.Admin,
  RoleNames.WarehouseManager,
  RoleNames.WarehouseSupervisor,
]

const PAGE_SIZE = 20

const STATUS_ITEMS: Record<string, string> = {
  [ALL_VALUE]: 'Tüm durumlar',
  Draft: 'Taslak',
  InProgress: 'Devam Ediyor',
  Completed: 'Tamamlandı',
}

function StatusBadge({ status }: { status: StockCountStatus }) {
  const styles: Record<StockCountStatus, string> = {
    Draft:
      'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
    InProgress:
      'bg-blue-100 text-blue-800 dark:bg-blue-900/40 dark:text-blue-300',
    Completed:
      'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
  }
  const labels: Record<StockCountStatus, string> = {
    Draft: 'Taslak',
    InProgress: 'Devam Ediyor',
    Completed: 'Tamamlandı',
  }

  return (
    <span
      className={cn(
        'rounded-full px-2 py-0.5 text-xs font-medium',
        styles[status],
      )}
    >
      {labels[status]}
    </span>
  )
}

export function StockCountsPage() {
  const canManage = useHasAnyRole(SESSION_MANAGE_ROLES)

  const [warehouseFilter, setWarehouseFilter] = useState<string | undefined>()
  const [statusFilter, setStatusFilter] = useState<
    StockCountStatus | undefined
  >()
  const [isCreateOpen, setIsCreateOpen] = useState(false)
  const [page, setPage] = useState(1)

  const { data: warehouses } = useWarehouses()
  const { data, isLoading } = useStockCounts({
    warehouseId: warehouseFilter,
    status: statusFilter,
    page,
    pageSize: PAGE_SIZE,
  })
  const stockCounts = data?.items
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  const warehouseItems = useMemo(
    () => ({
      [ALL_VALUE]: 'Tüm depolar',
      ...Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    }),
    [warehouses],
  )

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Sayım</h2>
        {canManage && (
          <Button onClick={() => setIsCreateOpen(true)}>
            <Plus /> Yeni Sayım
          </Button>
        )}
      </div>

      <div className="flex gap-2">
        <Select
          items={warehouseItems}
          value={warehouseFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) => {
            setWarehouseFilter(!value || value === ALL_VALUE ? undefined : value)
            setPage(1)
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder="Depo" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm depolar</SelectItem>
            {warehouses?.map((warehouse) => (
              <SelectItem key={warehouse.id} value={warehouse.id}>
                {warehouse.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select
          items={STATUS_ITEMS}
          value={statusFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) => {
            setStatusFilter(
              !value || value === ALL_VALUE
                ? undefined
                : (value as StockCountStatus),
            )
            setPage(1)
          }}
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm durumlar" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm durumlar</SelectItem>
            <SelectItem value="Draft">Taslak</SelectItem>
            <SelectItem value="InProgress">Devam Ediyor</SelectItem>
            <SelectItem value="Completed">Tamamlandı</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Depo</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Satır Sayısı</TableHead>
            <TableHead>Oluşturulma Tarihi</TableHead>
            <TableHead>Kapanış Tarihi</TableHead>
            <TableHead className="w-24">Detay</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={6}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {stockCounts?.map((stockCount) => (
            <TableRow key={stockCount.id}>
              <TableCell>{stockCount.warehouseName}</TableCell>
              <TableCell>
                <StatusBadge status={stockCount.status} />
              </TableCell>
              <TableCell>{stockCount.lines.length}</TableCell>
              <TableCell>
                {formatUtcDateTime(stockCount.createdAtUtc)}
              </TableCell>
              <TableCell>
                {stockCount.closedAtUtc
                  ? formatUtcDateTime(stockCount.closedAtUtc)
                  : '—'}
              </TableCell>
              <TableCell>
                <NavigateButton stockCountId={stockCount.id} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <PaginationControls
        page={page}
        totalPages={totalPages}
        totalCount={totalCount}
        pageSize={PAGE_SIZE}
        onPageChange={setPage}
      />

      <CreateStockCountDialog
        open={isCreateOpen}
        onOpenChange={setIsCreateOpen}
      />
    </div>
  )
}

function NavigateButton({ stockCountId }: { stockCountId: string }) {
  const navigate = useNavigate()

  return (
    <Button
      variant="ghost"
      size="sm"
      onClick={() => navigate(`/stock-counts/${stockCountId}`)}
    >
      Detay
    </Button>
  )
}

function CreateStockCountDialog({
  open,
  onOpenChange,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
}) {
  const navigate = useNavigate()
  const { data: warehouses } = useWarehouses()
  const createStockCount = useCreateStockCount()

  const [warehouseId, setWarehouseId] = useState('')

  const warehouseItems = useMemo(
    () =>
      Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    [warehouses],
  )

  function handleOpenChange(nextOpen: boolean) {
    if (nextOpen) setWarehouseId('')
    onOpenChange(nextOpen)
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!warehouseId) return

    createStockCount.mutate(
      { warehouseId },
      {
        onSuccess: (id) => {
          toast.success('Sayım oturumu oluşturuldu.')
          onOpenChange(false)
          navigate(`/stock-counts/${id}`)
        },
      },
    )
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <DialogTitle>Yeni Sayım</DialogTitle>

          <div className="space-y-1.5">
            <Label htmlFor="stock-count-warehouse">Depo</Label>
            <Select
              items={warehouseItems}
              value={warehouseId}
              onValueChange={(value: string | null) =>
                setWarehouseId(value ?? '')
              }
            >
              <SelectTrigger id="stock-count-warehouse" className="w-full">
                <SelectValue placeholder="Depo seçin" />
              </SelectTrigger>
              <SelectContent>
                {warehouses?.map((warehouse) => (
                  <SelectItem key={warehouse.id} value={warehouse.id}>
                    {warehouse.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </div>

          {createStockCount.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(createStockCount.error)}
            </p>
          )}

          <DialogFooter>
            <Button
              type="submit"
              disabled={!warehouseId || createStockCount.isPending}
            >
              {createStockCount.isPending ? 'Oluşturuluyor...' : 'Oluştur'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}
