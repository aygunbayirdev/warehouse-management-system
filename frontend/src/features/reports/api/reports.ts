import { useQuery } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'
import type { PagedResult } from '@/lib/pagination'

import type {
  StockCountVarianceFilters,
  StockCountVarianceReportRowDto,
  StockItemDto,
  StockLevelFilters,
  StockMovementDto,
  StockMovementFilters,
} from '../types'

export function useStockLevels(filters: StockLevelFilters) {
  return useQuery({
    queryKey: ['reports', 'stock-levels', filters],
    queryFn: async () => {
      const response = await apiClient.get<StockItemDto[]>('/stock', {
        params: filters,
      })
      return response.data
    },
  })
}

export function useStockMovements(filters: StockMovementFilters) {
  return useQuery({
    queryKey: ['reports', 'stock-movements', filters],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<StockMovementDto>>(
        '/stock/movements',
        { params: filters },
      )
      return response.data
    },
  })
}

export function useStockCountVarianceReport(filters: StockCountVarianceFilters) {
  return useQuery({
    queryKey: ['reports', 'stock-count-variance', filters],
    queryFn: async () => {
      const response = await apiClient.get<StockCountVarianceReportRowDto[]>(
        '/stock-counts/variance-report',
        { params: filters },
      )
      return response.data
    },
  })
}
