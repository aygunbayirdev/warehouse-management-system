import { NavLink, Outlet } from 'react-router-dom'

import { ThemeToggle } from '@/components/ThemeToggle'
import { Button } from '@/components/ui/button'
import { useCurrentUser } from '@/features/auth/api/useCurrentUser'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { useLogout } from '@/features/auth/api/useLogout'
import { RoleNames } from '@/features/auth/types'
import { cn } from '@/lib/utils'

type NavItem = {
  to: string
  label: string
}

const NAV_ITEMS: NavItem[] = [
  { to: '/', label: 'Panel' },
  { to: '/products', label: 'Ürünler' },
  { to: '/categories', label: 'Kategoriler' },
  { to: '/units-of-measure', label: 'Birimler' },
  { to: '/warehouses', label: 'Depolar' },
  { to: '/goods-receipts', label: 'Mal Kabul' },
  { to: '/goods-issues', label: 'Sevkiyat' },
  { to: '/stock-transfers', label: 'Transfer' },
  { to: '/stock-counts', label: 'Sayım' },
  { to: '/stock-count-adjustments', label: 'Sayım Düzeltmeleri' },
  { to: '/reports', label: 'Raporlar' },
]

function navLinkClassName({ isActive }: { isActive: boolean }) {
  return cn(
    'rounded-md px-3 py-2 text-sm hover:bg-muted',
    isActive && 'bg-muted font-medium',
  )
}

export function AppLayout() {
  const { data: user, isLoading } = useCurrentUser()
  const isAdmin = useHasAnyRole([RoleNames.Admin])
  const logout = useLogout()

  return (
    <div className="flex min-h-screen">
      <aside className="flex w-56 shrink-0 flex-col border-r p-4">
        <h1 className="mb-6 text-lg font-semibold">WMS</h1>
        <nav className="flex flex-col gap-1">
          {NAV_ITEMS.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.to === '/'}
              className={navLinkClassName}
            >
              {item.label}
            </NavLink>
          ))}
          {isAdmin && (
            <NavLink to="/admin" className={navLinkClassName}>
              Yönetim
            </NavLink>
          )}
        </nav>
      </aside>
      <div className="flex flex-1 flex-col">
        <header className="flex items-center justify-end gap-4 border-b p-4">
          <span className="text-sm text-muted-foreground">
            {isLoading
              ? 'Yükleniyor...'
              : user && `${user.firstName} ${user.lastName}`}
          </span>
          <ThemeToggle />
          <Button variant="outline" onClick={logout}>
            Çıkış Yap
          </Button>
        </header>
        <main className="flex-1 p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
