import { Link } from 'react-router-dom'

export default function TermosUso() {
  return (
    <div className="legal-page">
      <div className="legal-page__container">
        <Link to="/signup" className="legal-page__back">
          ← Voltar ao cadastro
        </Link>
        <h1 className="legal-page__title">Termos de Uso</h1>
        <p className="legal-page__updated">
          Última atualização: {new Date().toLocaleDateString('pt-BR')}
        </p>

        <section className="legal-page__section">
          <h2>1. Aceitação dos termos</h2>
          <p>
            Ao acessar e utilizar esta plataforma, você concorda em cumprir e estar vinculado a
            estes Termos de Uso. Se você não concordar com qualquer parte destes termos, não
            deverá utilizar nossos serviços.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>2. Descrição do serviço</h2>
          <p>
            A plataforma oferece funcionalidades de software como serviço (SaaS), incluindo
            autenticação, gestão de usuários e tenants, conforme a documentação e os recursos
            disponibilizados na aplicação.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>3. Cadastro e conta</h2>
          <p>
            Para utilizar determinadas funcionalidades, é necessário criar uma conta fornecendo
            informações verdadeiras e atualizadas. Você é responsável pela confidencialidade de
            sua senha e por todas as atividades realizadas em sua conta.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>4. Uso aceitável</h2>
          <p>
            Você concorda em não utilizar a plataforma para fins ilegais, fraudulentos ou que
            violem direitos de terceiros. É proibido tentar obter acesso não autorizado a
            sistemas, contas ou dados de outros usuários.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>5. Alterações</h2>
          <p>
            Reservamo-nos o direito de alterar estes Termos de Uso a qualquer momento. As
            alterações entram em vigor a partir da publicação nesta página. O uso continuado da
            plataforma após as alterações constitui aceitação dos novos termos.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>6. Contato</h2>
          <p>
            Em caso de dúvidas sobre estes Termos de Uso, entre em contato através dos canais
            disponibilizados na plataforma.
          </p>
        </section>
      </div>
    </div>
  )
}
