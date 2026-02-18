import { Link } from 'react-router-dom'

export default function PoliticaPrivacidade() {
  return (
    <div className="legal-page">
      <div className="legal-page__container">
        <Link to="/signup" className="legal-page__back">
          ← Voltar ao cadastro
        </Link>
        <h1 className="legal-page__title">Política de Privacidade</h1>
        <p className="legal-page__updated">
          Última atualização: {new Date().toLocaleDateString('pt-BR')}
        </p>

        <section className="legal-page__section">
          <h2>1. Informações que coletamos</h2>
          <p>
            Coletamos informações que você nos fornece diretamente, como nome, sobrenome, e-mail e
            senha (armazenada de forma criptografada) no momento do cadastro. Também podemos
            coletar dados de uso da plataforma e informações técnicas (endereço IP, tipo de
            navegador) para fins de segurança e melhoria do serviço.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>2. Como utilizamos suas informações</h2>
          <p>
            Utilizamos suas informações para fornecer e melhorar nossos serviços, autenticar
            usuários, cumprir obrigações legais e comunicar-nos com você quando necessário. Não
            vendemos seus dados pessoais a terceiros.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>3. Autenticação e terceiros</h2>
          <p>
            A autenticação é realizada por meio do Keycloak (provedor de identidade). O
            processamento de credenciais e dados de login está sujeito às políticas do provedor
            e a fluxos seguros (OAuth 2.0 / OpenID Connect). Recomendamos a leitura das políticas
            do provedor de identidade utilizado na plataforma.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>4. Proteção dos dados</h2>
          <p>
            Adotamos medidas técnicas e organizacionais para proteger seus dados contra acesso
            não autorizado, alteração, divulgação ou destruição. A comunicação com a plataforma
            pode ser criptografada (HTTPS) quando disponível.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>5. Seus direitos</h2>
          <p>
            Você pode solicitar acesso, correção ou exclusão dos seus dados pessoais, na medida
            permitida pela lei aplicável. Para exercer esses direitos ou esclarecer dúvidas sobre
            o tratamento dos seus dados, entre em contato conosco pelos canais indicados na
            plataforma.
          </p>
        </section>

        <section className="legal-page__section">
          <h2>6. Alterações nesta política</h2>
          <p>
            Podemos atualizar esta Política de Privacidade periodicamente. A data da última
            atualização será indicada no topo da página. O uso continuado da plataforma após
            alterações constitui aceitação da política revisada.
          </p>
        </section>
      </div>
    </div>
  )
}
