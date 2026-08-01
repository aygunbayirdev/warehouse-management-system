export type GoodsReceiptStatus = 'Draft' | 'Approved'

export type GoodsReceiptLineDto = {
  productId: string
  productSku: string
  productName: string
  quantity: number
}

export type GoodsReceiptDto = {
  id: string
  warehouseId: string
  warehouseName: string
  status: GoodsReceiptStatus
  createdByUserId: string
  createdAtUtc: string
  approvedAtUtc: string | null
  lines: GoodsReceiptLineDto[]
}

export type GoodsReceiptFilters = {
  warehouseId?: string
  status?: GoodsReceiptStatus
  page: number
  pageSize: number
}

export type CreateGoodsReceiptLineInput = {
  productId: string
  quantity: number
}

export type CreateGoodsReceiptPayload = {
  warehouseId: string
  lines: CreateGoodsReceiptLineInput[]
}
