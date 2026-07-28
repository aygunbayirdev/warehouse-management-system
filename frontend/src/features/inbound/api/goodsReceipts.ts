import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type {
  CreateGoodsReceiptPayload,
  GoodsReceiptDto,
  GoodsReceiptFilters,
} from '../types'

export function useGoodsReceipts(filters: GoodsReceiptFilters) {
  return useQuery({
    queryKey: ['goods-receipts', filters],
    queryFn: async () => {
      const response = await apiClient.get<GoodsReceiptDto[]>(
        '/goods-receipts',
        { params: filters },
      )
      return response.data
    },
  })
}

export function useCreateGoodsReceipt() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateGoodsReceiptPayload) => {
      const response = await apiClient.post<string>(
        '/goods-receipts',
        payload,
      )
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['goods-receipts'] })
    },
  })
}

export function useApproveGoodsReceipt() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/goods-receipts/${id}/approve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['goods-receipts'] })
    },
  })
}
