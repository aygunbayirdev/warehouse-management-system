import { type FormEvent, useState } from 'react'
import { useNavigate } from 'react-router-dom'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { getApiErrorMessage } from '@/lib/errors'

import { useLogin } from './api/useLogin'

export function LoginPage() {
  const [email, setEmail] = useState('admin@wms.local')
  const [password, setPassword] = useState('ChangeMe123!')
  const navigate = useNavigate()
  const loginMutation = useLogin()

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    loginMutation.mutate(
      { email, password },
      { onSuccess: () => navigate('/', { replace: true }) },
    )
  }

  return (
    <div className="flex min-h-screen items-center justify-center">
      <form
        onSubmit={handleSubmit}
        className="w-full max-w-sm space-y-4 rounded-lg border p-6"
      >
        <h1 className="text-lg font-semibold">WMS&apos;e Giriş Yap</h1>
        <p className="text-sm text-muted-foreground">
          Demo hesabı önceden dolduruldu, doğrudan &quot;Giriş Yap&quot;a
          tıklayabilirsiniz.
        </p>

        <div className="space-y-1.5">
          <Label htmlFor="email">E-posta</Label>
          <Input
            id="email"
            type="email"
            required
            autoComplete="username"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
          />
        </div>

        <div className="space-y-1.5">
          <Label htmlFor="password">Şifre</Label>
          <Input
            id="password"
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
          />
        </div>

        {loginMutation.isError && (
          <p className="text-sm text-destructive">
            {getApiErrorMessage(
              loginMutation.error,
              'Giriş yapılamadı. Lütfen tekrar deneyin.',
            )}
          </p>
        )}

        <Button type="submit" className="w-full" disabled={loginMutation.isPending}>
          {loginMutation.isPending ? 'Giriş yapılıyor...' : 'Giriş Yap'}
        </Button>
      </form>
    </div>
  )
}
