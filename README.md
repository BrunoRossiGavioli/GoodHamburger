# GoodHamburger

---

## Decisões técnicas

### 1. EFCore + SQLite

**Decisão:** Utilizar EFCore como ORM e SQLite como banco de dados durante o desenvolvimento inicial.

**Motivo:** Agilidade na configuração e prototipagem, sem necessidade de servidor de banco dedicado.

**Trade-off identificado:**  
O SQLite é, por padrão, **case sensitive** e **accent sensitive** (`cs` e `as`). Essa configuração não pode ser alterada após a criação do banco.  
Isso pode gerar comportamentos inesperados em buscas textuais (ex: `"nome" = "João"` não encontrar `"joão"`).

**Alternativa futura:**  
Migrar para **MySQL** ou **SQL Server**, que suportam collation `ci_ai` (case insensitive e accent insensitive), garantindo buscas mais previsíveis e compatíveis com regras de negócio comuns.

---

### 2. Repository Pattern + Service Pattern

**Decisão:** Adotar os padrões Repository e Service.

**Motivo:**  
- Seguir as recomendações da documentação oficial da Microsoft para ASP.NET Core.  
- Separar a lógica de acesso a dados (Repository) da lógica de negócio (Service).  
- Facilitar a aplicação de **testes unitários** por meio de injeção de dependência e mocks.

**Benefício direto:** Cada funcionalidade pode ser testada isoladamente sem depender de banco real ou integrações externas.

---

### 3. ProdutoEntity – Polimorfismo vs Enum

**Decisão:** Utilizar um `enum` para representar o tipo do produto, em vez de herança/polimorfismo.

**Motivo inicial para polimorfismo:** Tentativa de demonstrar conhecimento avançado de orientação a objetos.

**Análise crítica:**  
Ao avaliar melhor, percebi que nenhuma das subclasses adicionaria campos extras ou comportamentos significativamente diferentes. O uso de herança seria desnecessário e geraria complexidade sem ganho real.

**Decisão final:**  
- Uso de `enum TipoProduto`.  
- Código mais simples, direto e fácil de manter.  
- Não foi necessário usar **Newtonsoft.Json** para serialização polimórfica.

> *"Princípio KISS (Keep It ~~Simple, Stupid~~ Super Simple) aplicado."*

---

### 4. Autenticação com Identity (Duende)

**Decisão:** Utilizar o ASP.NET Core Identity para gerenciar autenticação e autorização.

**Motivo:**  
- Evitar reimplementar funcionalidades complexas como:  
  - Tabelas de usuários, roles e claims  
  - Validação de senha e e-mail  
  - Recuperação de senha, confirmação de e-mail, etc.  
- Adicionar segurança de forma consistente e confiável.  
- Não investir tempo reinventando a roda em um sistema bem consolidado (Identity da Duende).

---

### 5. Extension Methods

**Decisão:** Utilizar extension methods para transformações e mapeamentos auxiliares.

**Motivo:**  
- Respeitar o **Princípio da Responsabilidade Única (SRP)**.  
- Evitar poluir as classes de entidades e models com métodos de conversão ou lógica auxiliar.  
- Manter entidades focadas apenas no estado, e models focados na representação de dados.

**Organização resultante:**  
- Entidade = apenas dados persistidos.  
- Model/DTO = apenas dados de transporte.  
- Extension methods = lógica estática e reutilizável.

---

### 6. Uso de Models e DTOs

**Decisão:**  
- Criar **DtoRequest** para todas as requisições da API.  
- Utilizar diretamente os **Models** como retorno da API (sem DTOs específicos de resposta no momento).

**Justificativa para DTOs de requisição:**  
- Documentam claramente o que o cliente precisa enviar.  
- Evitam envio de campos desnecessários ou sensíveis.  
- Facilitam a validação de entrada.

**Justificativa para não usar DTOs de resposta ainda:**  
- Tamanho atual da aplicação não exige essa camada adicional.  
- À medida que a aplicação crescer e os retornos se tornarem mais complexos, será natural introduzir DTOs de resposta.  
- Evita over-engineering no momento atual.

> *"YAGNI (You Ain't Gonna Need It) aplicado para respostas, mas não para requisições."*

<br/>

### Resumo dos Trade-offs Assumidos

| Decisão | Trade-off assumido |
|---------|--------------------|
| SQLite | Case/accent sensitive – futuro: migrar para MySQL/SQL Server |
| Sem DTO de resposta | Menos flexibilidade futura, mas menos complexidade atual |
| Enum vs herança | Menos "flexibilidade teórica", mais simplicidade e clareza |

---