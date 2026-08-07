import { useMemo, useState } from 'react'
import { Link } from 'react-router-dom'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'

import { Card, CardAction, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { Tabs, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { useLowStockItems, usePendingApprovals } from '@/features/dashboard/api/dashboard'
import { useProducts } from '@/features/products/api/products'
import { useStockMovements } from '@/features/reports/api/reports'
import { useWarehouses } from '@/features/warehouses/api/warehouses'

const LOW_STOCK_LIMIT = 10

type MovementRange = '7g' | '1a' | '1y'

const MOVEMENT_RANGES: Record<MovementRange, { label: string; days: number; granularity: 'day' | 'month' }> = {
  '7g': { label: '7 Gün', days: 7, granularity: 'day' },
  '1a': { label: '1 Ay', days: 30, granularity: 'day' },
  '1y': { label: '1 Yıl', days: 365, granularity: 'month' },
}

const MONTH_LABELS = ['Oca', 'Şub', 'Mar', 'Nis', 'May', 'Haz', 'Tem', 'Ağu', 'Eyl', 'Eki', 'Kas', 'Ara']

function buildDayBuckets(days: number) {
  const buckets = new Map<string, { date: string; artis: number; azalis: number }>()

  for (let i = days - 1; i >= 0; i--) {
    const date = new Date()
    date.setDate(date.getDate() - i)
    const key = date.toISOString().slice(0, 10)
    buckets.set(key, { date: key, artis: 0, azalis: 0 })
  }

  return buckets
}

function buildMonthBuckets(months: number) {
  const buckets = new Map<string, { date: string; artis: number; azalis: number }>()
  const now = new Date()

  for (let i = months - 1; i >= 0; i--) {
    const date = new Date(now.getFullYear(), now.getMonth() - i, 1)
    const key = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`
    buckets.set(key, { date: key, artis: 0, azalis: 0 })
  }

  return buckets
}

function formatBucketLabel(value: string, granularity: 'day' | 'month') {
  if (granularity === 'month') {
    return MONTH_LABELS[Number(value.slice(5, 7)) - 1] ?? value
  }
  return value.slice(5)
}

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
  const [range, setRange] = useState<MovementRange>('7g')
  const { days, granularity } = MOVEMENT_RANGES[range]

  const fromUtc = useMemo(() => {
    const date = new Date()
    date.setDate(date.getDate() - days)
    return date.toISOString()
  }, [days])

  const { data } = useStockMovements({ fromUtc, page: 1, pageSize: 100 })

  const chartData = useMemo(() => {
    const buckets = granularity === 'month' ? buildMonthBuckets(12) : buildDayBuckets(days)
    const keyLength = granularity === 'month' ? 7 : 10

    for (const movement of data?.items ?? []) {
      const key = movement.occurredAtUtc.slice(0, keyLength)
      const bucket = buckets.get(key)
      if (!bucket) continue

      if (movement.type === 'Increase') {
        bucket.artis += movement.quantity
      } else {
        bucket.azalis += movement.quantity
      }
    }

    return Array.from(buckets.values())
  }, [data, days, granularity])

  return (
    <Card>
      <CardHeader>
        <CardTitle>Stok Hareketleri</CardTitle>
        <CardAction>
          <Tabs value={range} onValueChange={(value) => setRange(value as MovementRange)}>
            <TabsList>
              {Object.entries(MOVEMENT_RANGES).map(([value, config]) => (
                <TabsTrigger key={value} value={value}>
                  {config.label}
                </TabsTrigger>
              ))}
            </TabsList>
          </Tabs>
        </CardAction>
      </CardHeader>
      <CardContent>
        <ResponsiveContainer width="100%" height={260}>
          <BarChart data={chartData}>
            <CartesianGrid strokeDasharray="3 3" className="stroke-border" />
            <XAxis
              dataKey="date"
              tickFormatter={(value: string) => formatBucketLabel(value, granularity)}
              fontSize={12}
              tickLine={false}
              interval={granularity === 'day' && days > 7 ? 3 : 0}
            />
            <YAxis fontSize={12} tickLine={false} allowDecimals={false} />
            <Tooltip labelFormatter={(label) => formatBucketLabel(String(label), granularity)} />
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
