export type WarehouseDto = {
  id: string
  code: string
  name: string
  address: string | null
}

export type CreateWarehousePayload = {
  code: string
  name: string
  address: string | null
}

export type UpdateWarehousePayload = {
  name: string
  address: string | null
}
