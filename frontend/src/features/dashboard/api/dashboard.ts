import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'
import { useGoodsReceipts } from '@/features/inbound/api/goodsReceipts'
import { useGoodsIssues } from '@/features/outbound/api/goodsIssues'
import { useStockCountAdjustments } from '@/features/stock-count/api/stockCountAdjustments'
import { useStockTransfers } from '@/features/transfer/api/stockTransfers'

import type { LowStockItemDto, PendingApprovalItem } from '../types'

export function useLowStockItems(limit: number) {
  return useQuery({
    queryKey: ['dashboard', 'low-stock', limit],
    queryFn: async () => {
      const response = await apiClient.get<LowStockItemDto[]>('/stock/low-stock', {
        params: { limit },
      })
      return response.data
    },
  })
}

export function usePendingApprovals() {
  const draftReceipts = useGoodsReceipts({ status: 'Draft', page: 1, pageSize: 1 })
  const draftIssues = useGoodsIssues({ status: 'Draft', page: 1, pageSize: 1 })
  const draftTransfers = useStockTransfers({ status: 'Draft', page: 1, pageSize: 1 })
  const shippedTransfers = useStockTransfers({ status: 'Shipped', page: 1, pageSize: 1 })
  const pendingAdjustments = useStockCountAdjustments({ status: 'Pending', page: 1, pageSize: 1 })

  const items: PendingApprovalItem[] = [
    { label: 'Mal Kabul (Taslak)', count: draftReceipts.data?.totalCount ?? 0, to: '/goods-receipts' },
    { label: 'Sevkiyat (Taslak)', count: draftIssues.data?.totalCount ?? 0, to: '/goods-issues' },
    {
      label: 'Transfer (Taslak + Gönderildi)',
      count: (draftTransfers.data?.totalCount ?? 0) + (shippedTransfers.data?.totalCount ?? 0),
      to: '/stock-transfers',
    },
    {
      label: 'Sayım Düzeltme (Bekliyor)',
      count: pendingAdjustments.data?.totalCount ?? 0,
      to: '/stock-count-adjustments',
    },
  ]

  const total = items.reduce((sum, item) => sum + item.count, 0)

  return { items, total }
}
