export type StockItemDto = {
  warehouseId: string
  warehouseCode: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  unitOfMeasureCode: string
  quantity: number
}

export type StockLevelFilters = {
  warehouseId?: string
  productId?: string
}

export type StockMovementType = 'Increase' | 'Decrease'

export type StockMovementDto = {
  id: string
  warehouseId: string
  warehouseCode: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  type: StockMovementType
  quantity: number
  reason: string
  occurredAtUtc: string
}

export type StockMovementFilters = {
  warehouseId?: string
  productId?: string
  fromUtc?: string
  toUtc?: string
}

export type StockCountVarianceReportRowDto = {
  stockCountId: string
  warehouseId: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  systemQuantity: number
  countedQuantity: number
  difference: number
  closedAtUtc: string
}

export type StockCountVarianceFilters = {
  warehouseId?: string
  fromUtc?: string
  toUtc?: string
}
