import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type {
  CreateStockTransferPayload,
  StockTransferDto,
  StockTransferFilters,
} from '../types'

export function useStockTransfers(filters: StockTransferFilters) {
  return useQuery({
    queryKey: ['stock-transfers', filters],
    queryFn: async () => {
      const response = await apiClient.get<StockTransferDto[]>(
        '/stock-transfers',
        { params: filters },
      )
      return response.data
    },
  })
}

export function useCreateStockTransfer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateStockTransferPayload) => {
      const response = await apiClient.post<string>(
        '/stock-transfers',
        payload,
      )
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] })
    },
  })
}

export function useShipStockTransfer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-transfers/${id}/ship`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] })
    },
  })
}

export function useReceiveStockTransfer() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/stock-transfers/${id}/receive`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['stock-transfers'] })
    },
  })
}
