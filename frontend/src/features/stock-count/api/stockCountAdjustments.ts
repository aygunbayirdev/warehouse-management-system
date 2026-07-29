import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { StockCountAdjustmentDto, StockCountAdjustmentFilters } from '../types'

export function useStockCountAdjustments(filters: StockCountAdjustmentFilters) {
  return useQuery({
    queryKey: ['stock-count-adjustments', filters],
    queryFn: async () => {
      const response = await apiClient.get<StockCountAdjustmentDto[]>(
        '/stock-count-adjustments',
        { params: filters },
      )
      return response.data
    },
  })
}

export function useApproveStockCountAdjustment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-count-adjustments/${id}/approve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-count-adjustments'] })
    },
  })
}

export function useRejectStockCountAdjustment() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-count-adjustments/${id}/reject`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-count-adjustments'] })
    },
  })
}
