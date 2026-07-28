import { ThemeToggle } from '@/components/ThemeToggle'

export function DashboardPage() {
  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b p-4">
        <h1 className="text-lg font-semibold">WMS</h1>
        <ThemeToggle />
      </header>
      <main className="flex flex-1 items-center justify-center">
        <p className="text-muted-foreground">Dashboard (Faz 12)</p>
      </main>
    </div>
  )
}
