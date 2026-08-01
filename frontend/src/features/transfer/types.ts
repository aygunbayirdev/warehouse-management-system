export type StockTransferStatus = 'Draft' | 'Shipped' | 'Received' | 'Cancelled'

export type StockTransferLineDto = {
  productId: string
  productSku: string
  productName: string
  quantity: number
}

export type StockTransferDto = {
  id: string
  sourceWarehouseId: string
  sourceWarehouseName: string
  destinationWarehouseId: string
  destinationWarehouseName: string
  status: StockTransferStatus
  createdByUserId: string
  createdAtUtc: string
  shippedAtUtc: string | null
  receivedAtUtc: string | null
  lines: StockTransferLineDto[]
}

export type StockTransferFilters = {
  sourceWarehouseId?: string
  destinationWarehouseId?: string
  status?: StockTransferStatus
  page: number
  pageSize: number
}

export type CreateStockTransferLineInput = {
  productId: string
  quantity: number
}

export type CreateStockTransferPayload = {
  sourceWarehouseId: string
  destinationWarehouseId: string
  lines: CreateStockTransferLineInput[]
}
