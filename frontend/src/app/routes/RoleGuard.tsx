import type { ReactNode } from 'react'

import { useAuthStore } from '@/features/auth/store'

type RoleGuardProps = {
  allowedRoles: string[]
  children: ReactNode
}

export function RoleGuard({ allowedRoles, children }: RoleGuardProps) {
  const roles = useAuthStore((state) => state.user?.roles ?? [])
  const isAllowed = roles.some((role) => allowedRoles.includes(role))

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
