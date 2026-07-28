import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'

import { apiClient } from '@/lib/axios'

import type { UnitOfMeasureDto, UnitOfMeasurePayload } from '../types'

export function useUnitsOfMeasure() {
  return useQuery({
    queryKey: ['units-of-measure'],
    queryFn: async () => {
      const response = await apiClient.get<UnitOfMeasureDto[]>(
        '/units-of-measure',
      )
      return response.data
    },
  })
}

export function useCreateUnitOfMeasure() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (payload: UnitOfMeasurePayload) => {
      const response = await apiClient.post<string>(
        '/units-of-measure',
        payload,
      )
      return response.data
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units-of-measure'] })
    },
  })
}

export function useUpdateUnitOfMeasure() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async ({
      id,
      payload,
    }: {
      id: string
      payload: UnitOfMeasurePayload
    }) => {
      await apiClient.put(`/units-of-measure/${id}`, payload)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units-of-measure'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}

export function useDeleteUnitOfMeasure() {
  const queryClient = useQueryClient()

  return useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/units-of-measure/${id}`)
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['units-of-measure'] })
      queryClient.invalidateQueries({ queryKey: ['products'] })
    },
  })
}
