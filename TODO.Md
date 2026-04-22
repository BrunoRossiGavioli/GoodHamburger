# TODO

## API

### Produtos

- Criação do produto já cria um preço
- 
### Pedidos/Ordens

- Criar um pedido já cria também uma pedidoItem
- Criar em pedidos a propriedade/coluna de status (Pedido Realizado, Pedido em preparo, Pedido a caminho, Pedido entregue, Pedido Cancelado)
	- Motivo do cancelamento (opcional, mas pode ser útil para análise de dados e melhoria do serviço)

---

## Testes Unitários

- Criar testes unitários para validar as regras de negócio, como por exemplo:
  - Verificar se o preço do produto é atualizado corretamente quando um novo preço é criado
  - Verificar se o pedido é criado corretamente com os itens e preços correspondentes
  - Verificar se o total do pedido é calculado corretamente com base nos itens e preços
  - Etc. (Ainda escrever, mas só para ter uma ideia para não esquecer depois) 

---

## Frontend em Blazor

- Criar projeto Blazor server mode
- Instalar MudBlazor
- Aplicar o design do MudBlazor para criar uma interface amigável para o usuário
- Aplicar autenticação e autorização para proteger as rotas e recursos do aplicativo
- Chamar a API para login.
- Preparar single page para cadastro de pedidos
	- Semelhante ao layout do ifood onde a tela é dividida em duas partes, onde a parte esquerda tem a listagem de produtos e a parte direita tem o carrinho de compras, onde o usuário pode adicionar os produtos e visualizar o total do pedido.
- Tela para listagem de pedidos (Administrador)
	- Listagem de pedidos com detalhes como status, total, data, etc.
	- Opção para visualizar detalhes do pedido e alterar o status do pedido (ex: em andamento, concluído, cancelado)
- Criar telas de produtos (Administrador)
	- Listagem de produtos
	- Cadastro de produto
	- Visualização e edição de produto
		- Vai ter uma listagem de preços do produto, onde o usuário pode adicionar, editar e excluir preços