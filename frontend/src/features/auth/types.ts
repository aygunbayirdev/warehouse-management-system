export const RoleNames = {
  Admin: 'Admin',
  WarehouseManager: 'DepoMuduru',
  WarehouseSupervisor: 'DepoSorumlusu',
  WarehouseStaff: 'DepoPersoneli',
} as const

export type RoleName = (typeof RoleNames)[keyof typeof RoleNames]

export type AuthTokens = {
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
}

export type AuthenticatedUser = {
  id: string
  email: string
  firstName: string
  lastName: string
  roles: string[]
}
