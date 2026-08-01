import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'
import type { PagedResult } from '@/lib/pagination'

import type {
  CreateGoodsIssuePayload,
  GoodsIssueDto,
  GoodsIssueFilters,
} from '../types'

export function useGoodsIssues(filters: GoodsIssueFilters) {
  return useQuery({
    queryKey: ['goods-issues', filters],
    queryFn: async () => {
      const response = await apiClient.get<PagedResult<GoodsIssueDto>>(
        '/goods-issues',
        { params: filters },
      )
      return response.data
    },
  })
}

export function useCreateGoodsIssue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: CreateGoodsIssuePayload) => {
      const response = await apiClient.post<string>('/goods-issues', payload)
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['goods-issues'] })
    },
  })
}

export function useApproveGoodsIssue() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.post(`/goods-issues/${id}/approve`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['goods-issues'] })
    },
  })
}
