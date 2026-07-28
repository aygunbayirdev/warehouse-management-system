import { Plus } from 'lucide-react'
import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { toast } from 'sonner'

import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import { Button, buttonVariants } from '@/components/ui/button'
import { Dialog, DialogContent, DialogFooter, DialogTitle } from '@/components/ui/dialog'
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
import { cn } from '@/lib/utils'
import { formatUtcDateTime } from '@/lib/dates'
import { getApiErrorMessage } from '@/lib/errors'

import { useApproveGoodsIssue, useGoodsIssues } from './api/goodsIssues'
import type { GoodsIssueDto, GoodsIssueStatus } from './types'

const ALL_VALUE = 'all'
const APPROVE_ROLES = [
  RoleNames.Admin,
  RoleNames.WarehouseManager,
  RoleNames.WarehouseSupervisor,
]

const STATUS_ITEMS: Record<string, string> = {
  [ALL_VALUE]: 'Tüm durumlar',
  Draft: 'Taslak',
  Approved: 'Onaylandı',
}

function StatusBadge({ status }: { status: GoodsIssueStatus }) {
  const isApproved = status === 'Approved'
  return (
    <span
      className={cn(
        'rounded-full px-2 py-0.5 text-xs font-medium',
        isApproved
          ? 'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300'
          : 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/40 dark:text-yellow-300',
      )}
    >
      {isApproved ? 'Onaylandı' : 'Taslak'}
    </span>
  )
}

export function GoodsIssuesPage() {
  const canApprove = useHasAnyRole(APPROVE_ROLES)

  const [warehouseFilter, setWarehouseFilter] = useState<string | undefined>()
  const [statusFilter, setStatusFilter] = useState<
    GoodsIssueStatus | undefined
  >()

  const { data: warehouses } = useWarehouses()
  const { data: issues, isLoading } = useGoodsIssues({
    warehouseId: warehouseFilter,
    status: statusFilter,
  })

  const warehouseItems = useMemo(
    () => ({
      [ALL_VALUE]: 'Tüm depolar',
      ...Object.fromEntries(
        (warehouses ?? []).map((warehouse) => [warehouse.id, warehouse.name]),
      ),
    }),
    [warehouses],
  )

  const [viewingIssue, setViewingIssue] = useState<GoodsIssueDto | null>(null)
  const [approvingIssue, setApprovingIssue] = useState<GoodsIssueDto | null>(
    null,
  )

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Sevkiyat</h2>
        <Link
          to="/goods-issues/new"
          className={buttonVariants({ variant: 'default' })}
        >
          <Plus /> Yeni Sevkiyat
        </Link>
      </div>

      <div className="flex gap-2">
        <Select
          items={warehouseItems}
          value={warehouseFilter ?? ALL_VALUE}
          onValueChange={(value: string | null) =>
            setWarehouseFilter(!value || value === ALL_VALUE ? undefined : value)
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm depolar" />
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
          onValueChange={(value: string | null) =>
            setStatusFilter(
              !value || value === ALL_VALUE
                ? undefined
                : (value as GoodsIssueStatus),
            )
          }
        >
          <SelectTrigger>
            <SelectValue placeholder="Tüm durumlar" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value={ALL_VALUE}>Tüm durumlar</SelectItem>
            <SelectItem value="Draft">Taslak</SelectItem>
            <SelectItem value="Approved">Onaylandı</SelectItem>
          </SelectContent>
        </Select>
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Depo</TableHead>
            <TableHead>Hedef</TableHead>
            <TableHead>Durum</TableHead>
            <TableHead>Satır Sayısı</TableHead>
            <TableHead>Oluşturulma Tarihi</TableHead>
            <TableHead className="w-24">Detay</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={6}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {issues?.map((issue) => (
            <TableRow key={issue.id}>
              <TableCell>{issue.warehouseName}</TableCell>
              <TableCell>{issue.destination}</TableCell>
              <TableCell>
                <StatusBadge status={issue.status} />
              </TableCell>
              <TableCell>{issue.lines.length}</TableCell>
              <TableCell>{formatUtcDateTime(issue.createdAtUtc)}</TableCell>
              <TableCell>
                <Button
                  variant="ghost"
                  size="sm"
                  onClick={() => setViewingIssue(issue)}
                >
                  Detay
                </Button>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <Dialog
        open={viewingIssue !== null}
        onOpenChange={(open) => {
          if (!open) setViewingIssue(null)
        }}
      >
        <DialogContent>
          {viewingIssue && (
            <div className="space-y-4">
              <DialogTitle>Sevkiyat Detayı</DialogTitle>

              <div className="space-y-1 text-sm">
                <p>
                  <span className="text-muted-foreground">Depo: </span>
                  {viewingIssue.warehouseName}
                </p>
                <p>
                  <span className="text-muted-foreground">Hedef: </span>
                  {viewingIssue.destination}
                </p>
                <p>
                  <span className="text-muted-foreground">Durum: </span>
                  <StatusBadge status={viewingIssue.status} />
                </p>
                <p>
                  <span className="text-muted-foreground">
                    Oluşturulma Tarihi:{' '}
                  </span>
                  {formatUtcDateTime(viewingIssue.createdAtUtc)}
                </p>
                {viewingIssue.approvedAtUtc && (
                  <p>
                    <span className="text-muted-foreground">
                      Onay Tarihi:{' '}
                    </span>
                    {formatUtcDateTime(viewingIssue.approvedAtUtc)}
                  </p>
                )}
              </div>

              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead>SKU</TableHead>
                    <TableHead>Ürün</TableHead>
                    <TableHead>Miktar</TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {viewingIssue.lines.map((line) => (
                    <TableRow key={line.productId}>
                      <TableCell>{line.productSku}</TableCell>
                      <TableCell>{line.productName}</TableCell>
                      <TableCell>{line.quantity}</TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>

              {viewingIssue.status === 'Draft' && canApprove && (
                <DialogFooter>
                  <Button
                    onClick={() => {
                      setApprovingIssue(viewingIssue)
                      setViewingIssue(null)
                    }}
                  >
                    Onayla
                  </Button>
                </DialogFooter>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>

      <ApproveGoodsIssueDialog
        issue={approvingIssue}
        onOpenChange={(open) => {
          if (!open) setApprovingIssue(null)
        }}
      />
    </div>
  )
}

function ApproveGoodsIssueDialog({
  issue,
  onOpenChange,
}: {
  issue: GoodsIssueDto | null
  onOpenChange: (open: boolean) => void
}) {
  const approveGoodsIssue = useApproveGoodsIssue()

  function handleConfirm() {
    if (!issue) return

    approveGoodsIssue.mutate(issue.id, {
      onSuccess: () => {
        toast.success('Sevkiyat onaylandı.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={issue !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Sevkiyatı onayla</AlertDialogTitle>
        <AlertDialogDescription>
          Bu sevkiyatı onaylamak istediğinize emin misiniz? Onaylandığında
          ilgili depodaki stok miktarları otomatik olarak azaltılacaktır.
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={approveGoodsIssue.isPending}
          >
            Onayla
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
