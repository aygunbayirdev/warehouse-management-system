import type { ReactNode } from 'react'

import { useCurrentUser } from '@/features/auth/api/useCurrentUser'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'

type RoleGuardProps = {
  allowedRoles: string[]
  children: ReactNode
}

export function RoleGuard({ allowedRoles, children }: RoleGuardProps) {
  const { isLoading } = useCurrentUser()
  const isAllowed = useHasAnyRole(allowedRoles)

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center">
        <p className="text-muted-foreground">Yükleniyor...</p>
      </div>
    )
  }

  if (!isAllowed) {
    return (
      <div className="flex min-h-screen items-center justify-center text-center">
        <p className="text-muted-foreground">
          Bu sayfayı görüntüleme yetkiniz yok.
        </p>
      </div>
    )
  }

  return <>{children}</>
}
