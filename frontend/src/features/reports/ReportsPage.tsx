import { X } from 'lucide-react'
import { useMemo, useState } from 'react'

import { PaginationControls } from '@/components/PaginationControls'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { ProductLookupDialog } from '@/features/products/ProductLookupDialog'
import type { ProductDto } from '@/features/products/types'
import { useWarehouses } from '@/features/warehouses/api/warehouses'
import { formatUtcDateTime } from '@/lib/dates'
import { cn } from '@/lib/utils'

import {
  useStockCountVarianceReport,
  useStockLevels,
  useStockMovements,
} from './api/reports'
import type { StockMovementType } from './types'

const ALL_VALUE = 'all'
const PAGE_SIZE = 20

function WarehouseFilter({
  value,
  onChange,
}: {
  value: string | undefined
  onChange: (value: string | undefined) => void
}) {
  const { data: warehouses } = useWarehouses()

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
    <Select
      items={warehouseItems}
      value={value ?? ALL_VALUE}
      onValueChange={(next: string | null) =>
        onChange(!next || next === ALL_VALUE ? undefined : next)
      }
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
  )
}

function ProductFilter({
  product,
  onChange,
}: {
  product: { id: string; label: string } | null
  onChange: (product: { id: string; label: string } | null) => void
}) {
  const [isOpen, setIsOpen] = useState(false)

  return (
    <div className="flex items-center gap-1">
      <Button type="button" variant="outline" onClick={() => setIsOpen(true)}>
        {product?.label ?? 'Ürün Filtrele'}
      </Button>
      {product && (
        <Button
          type="button"
          variant="ghost"
          size="icon-sm"
          aria-label="Ürün filtresini temizle"
          onClick={() => onChange(null)}
        >
          <X />
        </Button>
      )}
      <ProductLookupDialog
        open={isOpen}
        onOpenChange={setIsOpen}
        onSelect={(selected: ProductDto) =>
          onChange({ id: selected.id, label: `${selected.name} (${selected.sku})` })
        }
      />
    </div>
  )
}

function DateRangeFilter({
  fromUtc,
  toUtc,
  onFromChange,
  onToChange,
}: {
  fromUtc: string | undefined
  toUtc: string | undefined
  onFromChange: (value: string | undefined) => void
  onToChange: (value: string | undefined) => void
}) {
  return (
    <div className="flex items-center gap-2">
      <div className="space-y-1.5">
        <Label htmlFor="date-from" className="text-xs text-muted-foreground">
          Başlangıç
        </Label>
        <Input
          id="date-from"
          type="date"
          value={fromUtc ?? ''}
          onChange={(event) => onFromChange(event.target.value || undefined)}
        />
      </div>
      <div className="space-y-1.5">
        <Label htmlFor="date-to" className="text-xs text-muted-foreground">
          Bitiş
        </Label>
        <Input
          id="date-to"
          type="date"
          value={toUtc ?? ''}
          onChange={(event) => onToChange(event.target.value || undefined)}
        />
      </div>
    </div>
  )
}

function DifferenceCell({ value }: { value: number }) {
  return (
    <span
      className={cn(
        value > 0 && 'text-green-600 dark:text-green-400',
        value < 0 && 'text-destructive',
      )}
    >
      {value > 0 ? `+${value}` : value}
    </span>
  )
}

function MovementTypeBadge({ type }: { type: StockMovementType }) {
  const styles: Record<StockMovementType, string> = {
    Increase:
      'bg-green-100 text-green-800 dark:bg-green-900/40 dark:text-green-300',
    Decrease:
      'bg-red-100 text-red-800 dark:bg-red-900/40 dark:text-red-300',
  }
  const labels: Record<StockMovementType, string> = {
    Increase: 'Artış',
    Decrease: 'Azalış',
  }

  return (
    <span
      className={cn('rounded-full px-2 py-0.5 text-xs font-medium', styles[type])}
    >
      {labels[type]}
    </span>
  )
}

function StockLevelsReport() {
  const [warehouseId, setWarehouseId] = useState<string | undefined>()
  const [product, setProduct] = useState<{ id: string; label: string } | null>(
    null,
  )

  const { data: stockLevels, isLoading } = useStockLevels({
    warehouseId,
    productId: product?.id,
  })

  return (
    <div className="space-y-4">
      <div className="flex gap-2">
        <WarehouseFilter value={warehouseId} onChange={setWarehouseId} />
        <ProductFilter product={product} onChange={setProduct} />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Depo</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead>Ürün</TableHead>
            <TableHead>Birim</TableHead>
            <TableHead>Miktar</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={5}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {!isLoading && stockLevels?.length === 0 && (
            <TableRow>
              <TableCell colSpan={5}>Kayıt bulunamadı.</TableCell>
            </TableRow>
          )}
          {stockLevels?.map((item) => (
            <TableRow key={`${item.warehouseId}-${item.productId}`}>
              <TableCell>{item.warehouseName}</TableCell>
              <TableCell>{item.productSku}</TableCell>
              <TableCell>{item.productName}</TableCell>
              <TableCell>{item.unitOfMeasureCode}</TableCell>
              <TableCell>{item.quantity}</TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function StockMovementsReport() {
  const [warehouseId, setWarehouseId] = useState<string | undefined>()
  const [product, setProduct] = useState<{ id: string; label: string } | null>(
    null,
  )
  const [fromUtc, setFromUtc] = useState<string | undefined>()
  const [toUtc, setToUtc] = useState<string | undefined>()
  const [page, setPage] = useState(1)

  const { data, isLoading } = useStockMovements({
    warehouseId,
    productId: product?.id,
    fromUtc,
    toUtc,
    page,
    pageSize: PAGE_SIZE,
  })
  const movements = data?.items
  const totalCount = data?.totalCount ?? 0
  const totalPages = Math.max(1, Math.ceil(totalCount / PAGE_SIZE))

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-2">
        <WarehouseFilter
          value={warehouseId}
          onChange={(value) => {
            setWarehouseId(value)
            setPage(1)
          }}
        />
        <ProductFilter
          product={product}
          onChange={(value) => {
            setProduct(value)
            setPage(1)
          }}
        />
        <DateRangeFilter
          fromUtc={fromUtc}
          toUtc={toUtc}
          onFromChange={(value) => {
            setFromUtc(value)
            setPage(1)
          }}
          onToChange={(value) => {
            setToUtc(value)
            setPage(1)
          }}
        />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Tarih</TableHead>
            <TableHead>Depo</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead>Ürün</TableHead>
            <TableHead>Tip</TableHead>
            <TableHead>Miktar</TableHead>
            <TableHead>Neden</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={7}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {!isLoading && movements?.length === 0 && (
            <TableRow>
              <TableCell colSpan={7}>Kayıt bulunamadı.</TableCell>
            </TableRow>
          )}
          {movements?.map((movement) => (
            <TableRow key={movement.id}>
              <TableCell>{formatUtcDateTime(movement.occurredAtUtc)}</TableCell>
              <TableCell>{movement.warehouseName}</TableCell>
              <TableCell>{movement.productSku}</TableCell>
              <TableCell>{movement.productName}</TableCell>
              <TableCell>
                <MovementTypeBadge type={movement.type} />
              </TableCell>
              <TableCell>{movement.quantity}</TableCell>
              <TableCell>{movement.reason}</TableCell>
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
    </div>
  )
}

function StockCountVarianceReportTab() {
  const [warehouseId, setWarehouseId] = useState<string | undefined>()
  const [fromUtc, setFromUtc] = useState<string | undefined>()
  const [toUtc, setToUtc] = useState<string | undefined>()

  const { data: rows, isLoading } = useStockCountVarianceReport({
    warehouseId,
    fromUtc,
    toUtc,
  })

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-end gap-2">
        <WarehouseFilter value={warehouseId} onChange={setWarehouseId} />
        <DateRangeFilter
          fromUtc={fromUtc}
          toUtc={toUtc}
          onFromChange={setFromUtc}
          onToChange={setToUtc}
        />
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Kapanış Tarihi</TableHead>
            <TableHead>Depo</TableHead>
            <TableHead>SKU</TableHead>
            <TableHead>Ürün</TableHead>
            <TableHead>Sistem Miktarı</TableHead>
            <TableHead>Sayılan Miktar</TableHead>
            <TableHead>Fark</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={7}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {!isLoading && rows?.length === 0 && (
            <TableRow>
              <TableCell colSpan={7}>Kayıt bulunamadı.</TableCell>
            </TableRow>
          )}
          {rows?.map((row) => (
            <TableRow key={`${row.stockCountId}-${row.productId}`}>
              <TableCell>{formatUtcDateTime(row.closedAtUtc)}</TableCell>
              <TableCell>{row.warehouseName}</TableCell>
              <TableCell>{row.productSku}</TableCell>
              <TableCell>{row.productName}</TableCell>
              <TableCell>{row.systemQuantity}</TableCell>
              <TableCell>{row.countedQuantity}</TableCell>
              <TableCell>
                <DifferenceCell value={row.difference} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

export function ReportsPage() {
  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">Raporlar</h2>

      <Tabs defaultValue="stock-levels">
        <TabsList>
          <TabsTrigger value="stock-levels">Stok Durumu</TabsTrigger>
          <TabsTrigger value="stock-movements">Stok Hareketleri</TabsTrigger>
          <TabsTrigger value="stock-count-variance">Sayım Farkı</TabsTrigger>
        </TabsList>
        <TabsContent value="stock-levels">
          <StockLevelsReport />
        </TabsContent>
        <TabsContent value="stock-movements">
          <StockMovementsReport />
        </TabsContent>
        <TabsContent value="stock-count-variance">
          <StockCountVarianceReportTab />
        </TabsContent>
      </Tabs>
    </div>
  )
}
