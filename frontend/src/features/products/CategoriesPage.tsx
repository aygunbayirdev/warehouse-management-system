import { Pencil, Plus, Trash2 } from 'lucide-react'
import { type FormEvent, useState } from 'react'
import { toast } from 'sonner'

import { Button } from '@/components/ui/button'
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogTitle,
} from '@/components/ui/alert-dialog'
import {
  Dialog,
  DialogContent,
  DialogFooter,
  DialogTitle,
} from '@/components/ui/dialog'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'
import { useHasAnyRole } from '@/features/auth/api/useHasAnyRole'
import { RoleNames } from '@/features/auth/types'
import { getApiErrorMessage } from '@/lib/errors'

import {
  useCategories,
  useCreateCategory,
  useDeleteCategory,
  useUpdateCategory,
} from './api/categories'
import type { CategoryDto } from './types'

export function CategoriesPage() {
  const { data: categories, isLoading } = useCategories()
  const canManage = useHasAnyRole([RoleNames.Admin, RoleNames.WarehouseManager])

  const [editingCategory, setEditingCategory] = useState<CategoryDto | null>(
    null,
  )
  const [isFormOpen, setIsFormOpen] = useState(false)
  const [deletingCategory, setDeletingCategory] = useState<CategoryDto | null>(
    null,
  )

  function openCreateForm() {
    setEditingCategory(null)
    setIsFormOpen(true)
  }

  function openEditForm(category: CategoryDto) {
    setEditingCategory(category)
    setIsFormOpen(true)
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h2 className="text-xl font-semibold">Kategoriler</h2>
        {canManage && (
          <Button onClick={openCreateForm}>
            <Plus /> Yeni Kategori
          </Button>
        )}
      </div>

      <Table>
        <TableHeader>
          <TableRow>
            <TableHead>Ad</TableHead>
            {canManage && <TableHead className="w-24">Aksiyonlar</TableHead>}
          </TableRow>
        </TableHeader>
        <TableBody>
          {isLoading && (
            <TableRow>
              <TableCell colSpan={2}>Yükleniyor...</TableCell>
            </TableRow>
          )}
          {categories?.map((category) => (
            <TableRow key={category.id}>
              <TableCell>{category.name}</TableCell>
              {canManage && (
                <TableCell>
                  <div className="flex gap-1">
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Düzenle"
                      onClick={() => openEditForm(category)}
                    >
                      <Pencil />
                    </Button>
                    <Button
                      variant="ghost"
                      size="icon-sm"
                      aria-label="Sil"
                      onClick={() => setDeletingCategory(category)}
                    >
                      <Trash2 />
                    </Button>
                  </div>
                </TableCell>
              )}
            </TableRow>
          ))}
        </TableBody>
      </Table>

      <CategoryFormDialog
        key={editingCategory?.id ?? 'new'}
        open={isFormOpen}
        onOpenChange={setIsFormOpen}
        category={editingCategory}
      />

      <DeleteCategoryDialog
        category={deletingCategory}
        onOpenChange={(open) => {
          if (!open) setDeletingCategory(null)
        }}
      />
    </div>
  )
}

function CategoryFormDialog({
  open,
  onOpenChange,
  category,
}: {
  open: boolean
  onOpenChange: (open: boolean) => void
  category: CategoryDto | null
}) {
  const [name, setName] = useState(category?.name ?? '')
  const createCategory = useCreateCategory()
  const updateCategory = useUpdateCategory()
  const mutation = category ? updateCategory : createCategory

  function handleOpenChange(nextOpen: boolean) {
    if (nextOpen) {
      setName(category?.name ?? '')
      mutation.reset()
    }
    onOpenChange(nextOpen)
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const payload = { name }

    if (category) {
      updateCategory.mutate(
        { id: category.id, payload },
        {
          onSuccess: () => {
            toast.success('Kategori güncellendi.')
            onOpenChange(false)
          },
        },
      )
    } else {
      createCategory.mutate(payload, {
        onSuccess: () => {
          toast.success('Kategori oluşturuldu.')
          onOpenChange(false)
        },
      })
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent>
        <form onSubmit={handleSubmit} className="space-y-4">
          <DialogTitle>
            {category ? 'Kategoriyi Düzenle' : 'Yeni Kategori'}
          </DialogTitle>

          <div className="space-y-1.5">
            <Label htmlFor="category-name">Ad</Label>
            <Input
              id="category-name"
              required
              maxLength={100}
              value={name}
              onChange={(event) => setName(event.target.value)}
            />
          </div>

          {mutation.isError && (
            <p className="text-sm text-destructive">
              {getApiErrorMessage(mutation.error)}
            </p>
          )}

          <DialogFooter>
            <Button type="submit" disabled={mutation.isPending}>
              {mutation.isPending ? 'Kaydediliyor...' : 'Kaydet'}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  )
}

function DeleteCategoryDialog({
  category,
  onOpenChange,
}: {
  category: CategoryDto | null
  onOpenChange: (open: boolean) => void
}) {
  const deleteCategory = useDeleteCategory()

  function handleConfirm() {
    if (!category) return

    deleteCategory.mutate(category.id, {
      onSuccess: () => {
        toast.success('Kategori silindi.')
        onOpenChange(false)
      },
      onError: (error) => {
        toast.error(getApiErrorMessage(error))
        onOpenChange(false)
      },
    })
  }

  return (
    <AlertDialog open={category !== null} onOpenChange={onOpenChange}>
      <AlertDialogContent>
        <AlertDialogTitle>Kategoriyi sil</AlertDialogTitle>
        <AlertDialogDescription>
          &quot;{category?.name}&quot; kategorisini silmek istediğinize emin
          misiniz?
        </AlertDialogDescription>
        <AlertDialogFooter>
          <AlertDialogCancel>Vazgeç</AlertDialogCancel>
          <AlertDialogAction
            onClick={handleConfirm}
            disabled={deleteCategory.isPending}
          >
            Sil
          </AlertDialogAction>
        </AlertDialogFooter>
      </AlertDialogContent>
    </AlertDialog>
  )
}
