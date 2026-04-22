## Atualizar Readme
Agora que a API está pronta, vou atualizar o Readme com os motivos das decisões de arquitetura, tecnologias utilizadas, e como rodar o projeto.

---

## Testes Unitários

- Criar testes unitário para alteração de status da ordem

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