export type LowStockItemDto = {
  warehouseId: string
  warehouseCode: string
  warehouseName: string
  productId: string
  productSku: string
  productName: string
  unitOfMeasureCode: string
  quantity: number
  minStockQuantity: number
}

export type PendingApprovalItem = {
  label: string
  count: number
  to: string
}
