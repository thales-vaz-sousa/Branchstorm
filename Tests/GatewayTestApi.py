"""
Testes de aceitação das regras de negócio do Branchstorm (README, RF01 a RF12
e seção 1.4), executados contra o gateway publicado.

Execução:
    python -m pytest -v test.py

Outro ambiente:
    BASE_URL=http://localhost:10000 python -m pytest -v test.py

Os nomes dos campos dos payloads seguem as entidades descritas no README.
Se um teste retornar 400 em POST/PUT, confira o DTO correspondente em
*ClassLibrary/*/Entities/Dtos e ajuste o builder respectivo.
"""

import os
import time
import uuid

import pytest
import requests


# ---------------------------------------------------------------- configuração

BASE_URL = os.getenv("BASE_URL", "https://branchstorm.onrender.com").rstrip("/")
TIMEOUT = 60

USER_PATH = "/user/api/user"
TEAM_PATH = "/team/api/team"
IDEA_PATH = "/idea/api/idea"
HYPOTHESIS_PATH = "/hypothesis/api/hypothesis"
FEEDBACK_PATH = "/feedback/api/feedbacks"
EXPORT_PATH = "/export/api/export"

ROLES = ["Admin", "Owner", "Collaborator", "Evaluator"]  # README 2.2.2
MAX_BATCH = 100  # README: consultas em lote aceitam no máximo 100 códigos

_session = requests.Session()


# ---------------------------------------------------------------- cliente HTTP

def call(method, path, **kwargs):
    return _session.request(method, f"{BASE_URL}{path}", timeout=TIMEOUT, **kwargs)


def unique(prefix):
    return f"{prefix}-{uuid.uuid4().hex[:8]}"


def normalize(value):
    """Enums chegam em PascalCase (NotTested) e o README usa snake_case (not_tested)."""
    return str(value).lower().replace("_", "")


def missing_code():
    return f"inexistente-{uuid.uuid4().hex}"


# ---------------------------------------------------------------- builders

def user_payload(name="Pessoa Teste"):
    return {
        "code": unique("u"),
        "name": name,
        "email": f"{unique('mail')}@teste.com",
    }


def team_payload(members, name="Time Teste"):
    return {
        "code": unique("t"),
        "name": name,
        "description": "Equipe de teste",
        "members": members,
    }


def idea_payload(author, team, father=None, kind="solution"):
    return {
        "code": unique("i"),
        "type": kind,
        "title": f"{kind} {unique('titulo')}",
        "description": "Descrição de teste",
        "fatherCode": father["code"] if father else None,
        "autorCode": author["code"],
        "teamCode": team["code"],
        "reason": "Motivo de teste",
    }


def hypothesis_payload(link_code):
    return {
        "code": unique("h"),
        "linkCode": link_code,
        "description": "Hipótese de teste",
        "successCriteria": "Critério de sucesso de teste",
    }


def feedback_payload(link_code, author):
    return {
        "linkCode": link_code,
        "autorCode": author["code"],
        "text": "Comentário de teste",
        "rating": 4,
    }


# ---------------------------------------------------------------- operações

def create_user(name="Pessoa Teste"):
    r = call("POST", USER_PATH, json=user_payload(name))
    assert r.status_code == 200, r.text
    return r.json()


def create_team(author, role="Owner"):
    members = [{"userCode": author["code"], "role": role}]
    r = call("POST", TEAM_PATH, json=team_payload(members))
    assert r.status_code == 200, r.text
    return r.json()


def create_root(author, team):
    r = call("POST", IDEA_PATH, json=idea_payload(author, team, kind="problem"))
    assert r.status_code == 200, r.text
    return r.json()


def create_child(parent, author, team, kind="solution"):
    r = call("POST", IDEA_PATH, json=idea_payload(author, team, parent, kind))
    assert r.status_code == 200, r.text
    return r.json()


def get_idea(code):
    r = call("GET", f"{IDEA_PATH}/{code}")
    assert r.status_code == 200, r.text
    return r.json()


def get_tree(root_code):
    r = call("GET", f"{IDEA_PATH}/tree/{root_code}")
    assert r.status_code == 200, r.text
    return r.json()


def select(code):
    r = call("PUT", f"{IDEA_PATH}/select/{code}")
    assert r.status_code == 200, r.text
    return r.json()


def create_hypothesis(link_code):
    r = call("POST", HYPOTHESIS_PATH, json=hypothesis_payload(link_code))
    assert r.status_code == 200, r.text
    return r.json()


# ---------------------------------------------------------------- fixtures

@pytest.fixture(scope="session", autouse=True)
def wait_for_gateway():
    """O plano free do Render adormece; aguarda o gateway responder."""
    for _ in range(6):
        try:
            if call("GET", "/").status_code == 200:
                return
        except requests.RequestException:
            pass
        time.sleep(10)
    pytest.fail(f"{BASE_URL} não respondeu após o aquecimento")


@pytest.fixture
def author():
    return create_user()


@pytest.fixture
def team(author):
    return create_team(author)


@pytest.fixture
def root(author, team):
    return create_root(author, team)


@pytest.fixture
def two_children(author, team, root):
    return create_child(root, author, team), create_child(root, author, team)


# ---------------------------------------------------------------- RF01: usuários

def test_rf01_cadastra_usuario():
    assert create_user()["code"]


def test_rf01_consulta_usuario_por_codigo(author):
    r = call("GET", f"{USER_PATH}/{author['code']}")
    assert r.status_code == 200
    assert r.json()["name"] == author["name"]


def test_rf01_usuario_inexistente_retorna_404():
    r = call("GET", f"{USER_PATH}/{missing_code()}")
    assert r.status_code == 404


def test_rf01_lista_usuarios_em_lote(author):
    other = create_user("Outra Pessoa")
    r = call("GET", USER_PATH, params={"codes": [author["code"], other["code"]]})
    assert r.status_code == 200
    returned = {u["code"] for u in r.json()}
    assert {author["code"], other["code"]} <= returned


def test_rf01_lote_de_usuarios_respeita_limite():
    codes = [f"x{i}" for i in range(MAX_BATCH + 1)]
    r = call("GET", USER_PATH, params={"codes": codes})
    assert r.status_code == 400


# ---------------------------------------------------------------- RF02: equipes e papéis

@pytest.mark.parametrize("role", ROLES)
def test_rf02_aceita_cada_papel(author, role):
    assert create_team(author, role=role)["code"]


@pytest.mark.xfail(
    strict=True,
    reason="README 2.3: GET de equipe deveria trazer nome dos membros via Usuários. "
           "O retorno atual traz somente userCode e role.",
)
def test_rf02_consulta_equipe_traz_dados_dos_membros(author, team):
    r = call("GET", f"{TEAM_PATH}/{team['code']}")
    assert r.status_code == 200
    assert author["name"] in r.text


def test_rf02_adiciona_membro(author, team):
    other = create_user("Novo Membro")
    payload = {
        "code": team["code"],
        "name": team["name"],
        "description": team["description"],
        "members": [
            {"userCode": author["code"], "role": "Owner"},
            {"userCode": other["code"], "role": "Collaborator"},
        ],
    }
    r = call("PATCH", f"{TEAM_PATH}/{team['code']}", json=payload)
    assert r.status_code == 200, r.text
    assert other["code"] in r.text


def test_rf02_equipe_inexistente_retorna_404():
    r = call("GET", f"{TEAM_PATH}/{missing_code()}")
    assert r.status_code == 404


# ---------------------------------------------------------------- RF03 e RF04: raiz e filhos

def test_rf03_cadastra_problema_raiz(root):
    assert root["code"]
    assert root.get("fatherCode") in (None, "")


def test_rf04_proposta_filha_registra_motivo(author, team, root):
    child = create_child(root, author, team)
    assert child["fatherCode"] == root["code"]
    assert child["reason"] == "Motivo de teste"


def test_rf04_versao_e_filha_de_solucao(author, team, root):
    solution = create_child(root, author, team, kind="solution")
    version = create_child(solution, author, team, kind="version")
    assert version["fatherCode"] == solution["code"]


def test_no_inexistente_retorna_404():
    r = call("GET", f"{IDEA_PATH}/{missing_code()}")
    assert r.status_code == 404


# ---------------------------------------------------------------- RF05 e seção 1.4: seleção

def test_rf05_selecionar_marca_no_e_descarta_irmao(root, two_children):
    a, b = two_children
    select(a["code"])

    assert normalize(get_idea(a["code"])["status"]) == "selected"
    assert normalize(get_idea(b["code"])["status"]) == "discarded"


def test_1_4_apenas_um_filho_selecionado_por_nivel(root, two_children):
    a, b = two_children
    select(a["code"])
    select(b["code"])

    assert normalize(get_idea(b["code"])["status"]) == "selected"
    assert normalize(get_idea(a["code"])["status"]) != "selected"


# ---------------------------------------------------------------- RF06 e seção 1.4: árvore

def test_rf06_arvore_inclui_nos_descartados(root, two_children):
    a, b = two_children
    select(a["code"])

    codes = {node["code"] for node in get_tree(root["code"])}
    assert {root["code"], a["code"], b["code"]} <= codes


def test_1_4_descartados_nao_sao_apagados(root, two_children):
    a, b = two_children
    select(a["code"])

    node_b = next(n for n in get_tree(root["code"]) if n["code"] == b["code"])
    assert normalize(node_b["status"]) == "discarded"


def test_rf06_status_dos_nos_sao_validos(root, two_children):
    valid = {normalize(s) for s in ["open", "selected", "discarded", "final"]}  # README 2.2.3
    for node in get_tree(root["code"]):
        assert normalize(node["status"]) in valid


# ---------------------------------------------------------------- RF07 a RF09: hipóteses

def test_rf07_cria_hipotese_no_no(root, two_children):
    a, _ = two_children
    hypothesis = create_hypothesis(a["code"])
    assert hypothesis["linkCode"] == a["code"]


def test_rf07_consulta_hipoteses_por_linkCode(root, two_children):
    a, _ = two_children
    hypothesis = create_hypothesis(a["code"])

    r = call("GET", HYPOTHESIS_PATH, params={"linkCode": a["code"]})
    assert r.status_code == 200
    assert hypothesis["code"] in [h["code"] for h in r.json()]


def test_rf07_consulta_sem_parametros_retorna_400():
    r = call("GET", HYPOTHESIS_PATH)
    assert r.status_code == 400


def test_rf07_lote_de_hipoteses_respeita_limite():
    r = call("GET", HYPOTHESIS_PATH, params={"linkCodes": [f"x{i}" for i in range(MAX_BATCH + 1)]})
    assert r.status_code == 400


@pytest.mark.parametrize(
    "result",
    [
        pytest.param(
            "refuted",
            id="refuted",
        ),
        pytest.param(
            "validate",
            id="validate",
            marks=pytest.mark.xfail(
                strict=True,
                reason="Bug: resultado 'validate' não atualiza a validação do nó "
                       "(continua NotTested). Verificar o enum de resultado.",
            ),
        ),
    ],
)
def test_rf08_rf09_resultado_atualiza_validacao_do_no(root, two_children, result):
    a, _ = two_children
    hypothesis = create_hypothesis(a["code"])

    r = call("PUT", f"{HYPOTHESIS_PATH}/result/{hypothesis['code']}", json={"result": result})
    assert r.status_code == 200, r.text

    assert normalize(get_idea(a["code"])["validation"]) == normalize(result)


@pytest.mark.xfail(
    strict=True,
    reason="Bug: resultado de hipótese inexistente retorna 500 em vez de 404. "
           "O serviço lança exceção em vez de retornar null.",
)
def test_rf08_resultado_de_hipotese_inexistente_retorna_404():
    r = call("PUT", f"{HYPOTHESIS_PATH}/result/{missing_code()}", json={"result": "validate"})
    assert r.status_code == 404


# ---------------------------------------------------------------- RF10 e RF11: feedback

def test_rf10_aceita_feedback_em_no_aberto(author, root, two_children):
    a, _ = two_children
    r = call("POST", FEEDBACK_PATH, json=feedback_payload(a["code"], author))
    assert r.status_code == 200, r.text


@pytest.mark.xfail(
    strict=True,
    reason="Bug (seção 1.4): feedback é aceito em nó descartado. "
           "Só deveria ser aceito em nós com status 'aberto'.",
)
def test_1_4_rejeita_feedback_em_no_descartado(author, root, two_children):
    a, b = two_children
    select(a["code"])  # b passa a ser descartado

    r = call("POST", FEEDBACK_PATH, json=feedback_payload(b["code"], author))
    assert r.status_code != 200


def test_rf11_lista_feedbacks_por_linkCodes(author, root, two_children):
    a, _ = two_children
    call("POST", FEEDBACK_PATH, json=feedback_payload(a["code"], author))

    r = call("GET", FEEDBACK_PATH, params={"linkCodes": [a["code"]]})
    assert r.status_code == 200
    assert any(f.get("linkCode") == a["code"] for f in r.json())


def test_rf11_consulta_sem_parametros_retorna_400():
    r = call("GET", FEEDBACK_PATH)
    assert r.status_code == 400


def test_rf11_lote_de_feedbacks_respeita_limite():
    r = call("GET", FEEDBACK_PATH, params={"linkCodes": [f"x{i}" for i in range(MAX_BATCH + 1)]})
    assert r.status_code == 400


@pytest.mark.xfail(
    strict=True,
    reason="README RF11 cita GET /api/feedbacks?linkCode=. O controller só aceita linkCodes.",
)
def test_rf11_lista_feedbacks_por_linkCode_singular(root, two_children):
    a, _ = two_children
    r = call("GET", FEEDBACK_PATH, params={"linkCode": a["code"]})
    assert r.status_code == 200


# ---------------------------------------------------------------- RF12: exportação

@pytest.mark.xfail(
    strict=True,
    reason="RF12: GET /api/export/{code} não está no nginx.conf nem nos controllers enviados.",
)
def test_rf12_exporta_arvore_somente_leitura(root):
    r = call("GET", f"{EXPORT_PATH}/{root['code']}")
    assert r.status_code == 200