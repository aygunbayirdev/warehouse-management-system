import { Link } from 'react-router-dom'

import { ThemeToggle } from '@/components/ThemeToggle'
import { Button } from '@/components/ui/button'
import { useCurrentUser } from '@/features/auth/api/useCurrentUser'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { useLogout } from '@/features/auth/api/useLogout'
import { RoleNames } from '@/features/auth/types'

export function DashboardPage() {
  const { data: user, isLoading } = useCurrentUser()
  const isAdmin = useHasAnyRole([RoleNames.Admin])
  const logout = useLogout()

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b p-4">
        <h1 className="text-lg font-semibold">WMS</h1>
        <div className="flex items-center gap-4">
          {isAdmin && <Link to="/admin">Yönetim</Link>}
          <span className="text-sm text-muted-foreground">
            {isLoading
              ? 'Yükleniyor...'
              : user && `${user.firstName} ${user.lastName}`}
          </span>
          <ThemeToggle />
          <Button variant="outline" onClick={logout}>
            Çıkış Yap
          </Button>
        </div>
      </header>
      <main className="flex flex-1 items-center justify-center">
        <p className="text-muted-foreground">Dashboard (Faz 12)</p>
      </main>
    </div>
  )
}
