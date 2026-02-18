import {
  Button,
  Input,
  Label,
  Title3,
  MessageBar,
  MessageBarBody,
  Checkbox,
  Spinner,
} from '@fluentui/react-components'
import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { registerUser } from '../api/register'

const EMAIL_REGEX = /^[^\s@]+@[^\s@]+\.[^\s@]+$/

export default function Signup() {
  const navigate = useNavigate()
  const [firstName, setFirstName] = useState('')
  const [lastName, setLastName] = useState('')
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [acceptTerms, setAcceptTerms] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState(false)
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setError(null)

    if (!firstName.trim()) {
      setError('Nome é obrigatório.')
      return
    }
    if (!lastName.trim()) {
      setError('Sobrenome é obrigatório.')
      return
    }
    if (!EMAIL_REGEX.test(email.trim())) {
      setError('Informe um e-mail válido.')
      return
    }
    if (password.length < 8) {
      setError('A senha deve ter no mínimo 8 caracteres.')
      return
    }
    if (password !== confirmPassword) {
      setError('A confirmação da senha não confere.')
      return
    }
    if (!acceptTerms) {
      setError('É necessário aceitar os Termos e a Política de Privacidade.')
      return
    }

    setSubmitting(true)
    try {
      await registerUser({
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        email: email.trim(),
        password,
        confirmPassword,
        acceptTerms,
      })
      setSuccess(true)
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Falha no cadastro.')
    } finally {
      setSubmitting(false)
    }
  }

  if (success) {
    return (
      <div className="page-center">
        <div className="card-admin">
          <Title3 style={{ margin: '0 0 0.5rem 0', fontSize: '1.5rem' }}>
            Conta criada
          </Title3>
          <p>Faça login para acessar a plataforma.</p>
          <Button appearance="primary" onClick={() => navigate('/login')}>
            Ir para login
          </Button>
        </div>
      </div>
    )
  }

  return (
    <div className="page-center">
      <div className="card-admin signup-card">
        <Title3 className="signup-card__title">Cadastro</Title3>
        <form className="signup-form" onSubmit={handleSubmit}>
          {error && (
            <MessageBar intent="error" className="signup-form__error">
              <MessageBarBody>{error}</MessageBarBody>
            </MessageBar>
          )}
          <div className="signup-form__field">
            <Label htmlFor="firstName">Nome</Label>
            <Input
              id="firstName"
              value={firstName}
              onChange={(_, d) => setFirstName(d.value)}
              placeholder="Nome"
              required
              disabled={submitting}
            />
          </div>
          <div className="signup-form__field">
            <Label htmlFor="lastName">Sobrenome</Label>
            <Input
              id="lastName"
              value={lastName}
              onChange={(_, d) => setLastName(d.value)}
              placeholder="Sobrenome"
              required
              disabled={submitting}
            />
          </div>
          <div className="signup-form__field">
            <Label htmlFor="email">E-mail</Label>
            <Input
              id="email"
              type="email"
              value={email}
              onChange={(_, d) => setEmail(d.value)}
              placeholder="seu@email.com"
              required
              disabled={submitting}
            />
          </div>
          <div className="signup-form__field">
            <Label htmlFor="password">Senha</Label>
            <Input
              id="password"
              type="password"
              value={password}
              onChange={(_, d) => setPassword(d.value)}
              placeholder="Mínimo 8 caracteres"
              required
              disabled={submitting}
            />
          </div>
          <div className="signup-form__field">
            <Label htmlFor="confirmPassword">Confirmação de senha</Label>
            <Input
              id="confirmPassword"
              type="password"
              value={confirmPassword}
              onChange={(_, d) => setConfirmPassword(d.value)}
              placeholder="Repita a senha"
              required
              disabled={submitting}
            />
          </div>
          <div className="signup-form__terms">
            <Checkbox
              id="acceptTerms"
              checked={acceptTerms}
              onChange={(_, d) => setAcceptTerms(!!d.checked)}
              label={
                <span className="signup-form__terms-label">
                  Aceito os{' '}
                  <Link to="/terms" target="_blank" rel="noopener noreferrer">
                    Termos de Uso
                  </Link>{' '}
                  e a{' '}
                  <Link to="/privacy" target="_blank" rel="noopener noreferrer">
                    Política de Privacidade
                  </Link>
                  .
                </span>
              }
              disabled={submitting}
            />
          </div>
          <div className="signup-form__actions">
            <Button
              appearance="primary"
              type="submit"
              disabled={submitting}
              icon={submitting ? <Spinner size="tiny" /> : undefined}
            >
              {submitting ? 'Cadastrando...' : 'Cadastrar'}
            </Button>
            <Link to="/login">
              <Button appearance="secondary" disabled={submitting}>
                Já tenho conta
              </Button>
            </Link>
          </div>
        </form>
      </div>
    </div>
  )
}
