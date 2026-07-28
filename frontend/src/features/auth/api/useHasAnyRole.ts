import { useCurrentUser } from './useCurrentUser'

export function useHasAnyRole(allowedRoles: string[]): boolean {
  const { data: user } = useCurrentUser()

  if (!user) {
    return false
  }

  return user.roles.some((role) => allowedRoles.includes(role))
}
