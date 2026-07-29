import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type {
  CreateStockCountPayload,
  StockCountDto,
  StockCountFilters,
  SubmitStockCountLinePayload,
} from '../types'

export function useStockCounts(filters: StockCountFilters) {
  return useQuery({
    queryKey: ['stock-counts', filters],
    queryFn: async () => {
      const response = await apiClient.get<StockCountDto[]>('/stock-counts', {
        params: filters,
      })
      return response.data
    },
  })
}

export function useStockCount(id: string) {
  return useQuery({
    queryKey: ['stock-counts', id],
    queryFn: async () => {
      const response = await apiClient.get<StockCountDto>(`/stock-counts/${id}`)
      return response.data
    },
    enabled: Boolean(id),
  })
}

export function useCreateStockCount() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateStockCountPayload) => {
      const response = await apiClient.post<string>('/stock-counts', payload)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-counts'] })
    },
  })
}

export function useStartStockCount() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-counts/${id}/start`)
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['stock-counts'] })
      queryClient.invalidateQueries({ queryKey: ['stock-counts', id] })
    },
  })
}

export function useSubmitStockCountLine(stockCountId: string) {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: SubmitStockCountLinePayload) => {
      await apiClient.post(`/stock-counts/${stockCountId}/lines`, payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-counts', stockCountId] })
    },
  })
}

export function useCompleteStockCount() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-counts/${id}/complete`)
    },
    onSuccess: (_data, id) => {
      queryClient.invalidateQueries({ queryKey: ['stock-counts'] })
      queryClient.invalidateQueries({ queryKey: ['stock-counts', id] })
    },
  })
}
