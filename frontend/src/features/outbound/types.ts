export type GoodsIssueStatus = 'Draft' | 'Approved'

export type GoodsIssueLineDto = {
  productId: string
  productSku: string
  productName: string
  quantity: number
}

export type GoodsIssueDto = {
  id: string
  warehouseId: string
  warehouseName: string
  destination: string
  status: GoodsIssueStatus
  createdByUserId: string
  createdAtUtc: string
  approvedAtUtc: string | null
  lines: GoodsIssueLineDto[]
}

export type GoodsIssueFilters = {
  warehouseId?: string
  status?: GoodsIssueStatus
  page: number
  pageSize: number
}

export type CreateGoodsIssueLineInput = {
  productId: string
  quantity: number
}

export type CreateGoodsIssuePayload = {
  warehouseId: string
  destination: string
  lines: CreateGoodsIssueLineInput[]
}
