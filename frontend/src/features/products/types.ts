export type ProductDto = {
  id: string
  sku: string
  name: string
  unitOfMeasureId: string
  unitOfMeasureCode: string
  categoryId: string | null
  categoryName: string | null
  minStockQuantity: number
}

export type CreateProductPayload = {
  sku: string
  name: string
  unitOfMeasureId: string
  categoryId: string | null
  minStockQuantity: number
}

export type UpdateProductPayload = {
  name: string
  unitOfMeasureId: string
  categoryId: string | null
  minStockQuantity: number
}

export type ProductFilters = {
  categoryId?: string
  search?: string
}

export type CategoryDto = {
  id: string
  name: string
}

export type CategoryPayload = {
  name: string
}

export type UnitOfMeasureDto = {
  id: string
  code: string
  name: string
}

export type UnitOfMeasurePayload = {
  code: string
  name: string
}
