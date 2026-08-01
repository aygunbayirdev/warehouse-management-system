export type StockCountStatus = 'Draft' | 'InProgress' | 'Completed'

export type StockCountLineDto = {
  productId: string
  productSku: string
  productName: string
  systemQuantity: number
  countedQuantity: number
  difference: number
}

export type StockCountDto = {
  id: string
  warehouseId: string
  warehouseName: string
  status: StockCountStatus
  createdByUserId: string
  createdAtUtc: string
  closedAtUtc: string | null
  lines: StockCountLineDto[]
}

export type StockCountFilters = {
  warehouseId?: string
  status?: StockCountStatus
  page: number
  pageSize: number
}

export type CreateStockCountPayload = {
  warehouseId: string
}

export type SubmitStockCountLinePayload = {
  productId: string
  countedQuantity: number
}

export type StockCountAdjustmentStatus = 'Pending' | 'Approved' | 'Rejected'

export type StockCountAdjustmentDto = {
  id: string
  stockCountId: string
  warehouseId: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  differenceQuantity: number
  status: StockCountAdjustmentStatus
  approvedByUserId: string | null
  createdAtUtc: string
  decidedAtUtc: string | null
}

export type StockCountAdjustmentFilters = {
  warehouseId?: string
  status?: StockCountAdjustmentStatus
  page: number
  pageSize: number
}
