import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'

import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useLowStockItems, usePendingApprovals } from '@/features/dashboard/api/dashboard'
import { useProducts } from '@/features/products/api/products'
import { useStockMovements } from '@/features/reports/api/reports'
import { useWarehouses } from '@/features/warehouses/api/warehouses'

const LOW_STOCK_LIMIT = 10
const MOVEMENTS_DAYS = 7

function StatCard({ title, value, to }: { title: string; value: number; to?: string }) {
  const content = (
    <Card className={to ? 'transition-colors hover:bg-accent/50' : undefined}>
      <CardHeader>
        <CardDescription>{title}</CardDescription>
        <CardTitle className="text-3xl">{value}</CardTitle>
      </CardHeader>
    </Card>
  )

  return to ? (
    <Link to={to} className="block">
      {content}
    </Link>
  ) : (
    content
  )
}

function StockMovementsChart() {
  const fromUtc = useMemo(() => {
    const date = new Date()
    date.setDate(date.getDate() - MOVEMENTS_DAYS)
    return date.toISOString()
  }, [])

  const { data } = useStockMovements({ fromUtc, page: 1, pageSize: 100 })

  const chartData = useMemo(() => {
    const buckets = new Map<string, { date: string; artis: number; azalis: number }>()

    for (let i = MOVEMENTS_DAYS - 1; i >= 0; i--) {
      const date = new Date()
      date.setDate(date.getDate() - i)
      const key = date.toISOString().slice(0, 10)
      buckets.set(key, { date: key, artis: 0, azalis: 0 })
    }

    for (const movement of data?.items ?? []) {
      const key = movement.occurredAtUtc.slice(0, 10)
      const bucket = buckets.get(key)
      if (!bucket) continue

      if (movement.type === 'Increase') {
        bucket.artis += movement.quantity
      } else {
        bucket.azalis += movement.quantity
      }
    }

    return Array.from(buckets.values())
  }, [data])

  return (
    <Card>
      <CardHeader>
        <CardTitle>Son 7 Gün Stok Hareketleri</CardTitle>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis
              dataKey="date"
              tickFormatter={(value: string) => value.slice(5)}
              fontSize={12}
              tickLine={false}
            />
            <YAxis fontSize={12} tickLine={false} allowDecimals={false} />
            <Tooltip />
            <Bar dataKey="artis" name="Artış" fill="#22c55e" radius={[4, 4, 0, 0]} />
            <Bar dataKey="azalis" name="Azalış" fill="#ef4444" radius={[4, 4, 0, 0]} />
          </BarChart>
        </ResponsiveContainer>
      </CardContent>
    </Card>
  )
}

function LowStockTable() {
  const { data, isLoading } = useLowStockItems(LOW_STOCK_LIMIT)

  return (
    <Card>
      <CardHeader>
        <CardTitle>Düşük Stok Uyarısı</CardTitle>
      </CardHeader>
      <CardContent>
        <Table>
          <TableHeader>
            <TableRow>
              <TableHead>Depo</TableHead>
              <TableHead>SKU</TableHead>
              <TableHead>Ürün</TableHead>
              <TableHead>Mevcut</TableHead>
              <TableHead>Min.</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isLoading && (
              <TableRow>
                <TableCell colSpan={5}>Yükleniyor...</TableCell>
              </TableRow>
            )}
            {!isLoading && data?.length === 0 && (
              <TableRow>
                <TableCell colSpan={5}>Düşük stokta ürün yok.</TableCell>
              </TableRow>
            )}
            {data?.map((item) => (
              <TableRow key={`${item.warehouseId}-${item.productId}`}>
                <TableCell>{item.warehouseName}</TableCell>
                <TableCell>{item.productSku}</TableCell>
                <TableCell>{item.productName}</TableCell>
                <TableCell className="text-destructive">{item.quantity}</TableCell>
                <TableCell>{item.minStockQuantity}</TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </CardContent>
    </Card>
  )
}

function PendingApprovalsList() {
  const { items } = usePendingApprovals()

  return (
    <Card>
      <CardHeader>
        <CardTitle>Bekleyen Onaylar</CardTitle>
      </CardHeader>
      <CardContent className="space-y-2">
        {items.map((item) => (
          <div
            key={item.label}
            className="flex items-center justify-between border-b border-border py-2 last:border-0"
          >
            <span>{item.label}</span>
            <div className="flex items-center gap-3">
              <span className="font-medium">{item.count}</span>
              <Link to={item.to} className="text-sm text-muted-foreground underline">
                Görüntüle
              </Link>
            </div>
          </div>
        ))}
      </CardContent>
    </Card>
  )
}

export function DashboardPage() {
  const { data: products } = useProducts({ page: 1, pageSize: 1 })
  const { data: warehouses } = useWarehouses()
  const { data: lowStockItems } = useLowStockItems(LOW_STOCK_LIMIT)
  const { total: pendingApprovalsTotal } = usePendingApprovals()

  return (
    <div className="space-y-4">
      <h2 className="text-xl font-semibold">Panel</h2>

      <div data-testid="stat-cards" className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard title="Toplam Ürün" value={products?.totalCount ?? 0} to="/products" />
        <StatCard title="Toplam Depo" value={warehouses?.length ?? 0} to="/warehouses" />
        <StatCard title="Bekleyen Onaylar" value={pendingApprovalsTotal} />
        <StatCard title="Düşük Stok Uyarısı" value={lowStockItems?.length ?? 0} />
      </div>

      <div className="grid gap-4 lg:grid-cols-2">
        <StockMovementsChart />
        <LowStockTable />
      </div>

      <PendingApprovalsList />
    </div>
  )
}
