# Test Technique - Développeur Fullstack Counteo

## Objectif du test

Développer une application console en .NET qui :

1. Parse un fichier CAMT.053 (fourni avec ce test)
2. Extrait les transactions avec leurs libellés
3. Utilise un service LLM (au choix) pour améliorer la lisibilité des libellés
4. Enregistre ou affiche les résultats de manière structurée

## Détails techniques

### Partie 1 : Parsing du fichier CAMT.053 (1-2h)

- Créer un parser pour extraire les informations pertinentes du fichier XML fourni
- Nous nous intéressons principalement aux champs suivants :
  - Date de la transaction
  - Montant et devise
  - Libellé
  - Référence de la transaction
  - Type d'opération (crédit/débit)

### Partie 2 : Harmonisation des libellés via LLM (1-2h)

- Intégrer un service LLM au choix (OpenAI, Anthropic/Claude, Mistral, Google Gemini, etc.)
- Note : Gemini de Google offre des crédits gratuits qui peuvent être suffisants pour ce test
- Concevoir un prompt efficace pour transformer les libellés bancaires en descriptions claires et cohérentes
- Gérer les appels API de manière efficiente (regroupement, rate limiting, etc.)

### Partie 3 : Structuration et présentation (1h)

- Organiser les données traitées de manière cohérente
- Permettre l'export ou l'affichage des résultats
- Documenter les choix techniques et les possibilités d'amélioration

## Livrable attendu

- Code source complet et documenté
- Instructions pour exécuter l'application
- Un court document (README) expliquant :
  - Votre approche et vos choix techniques
  - Les difficultés rencontrées et comment vous les avez résolues
  - Les améliorations que vous auriez apportées avec plus de temps

## Ressources fournies

- Un exemple de fichier CAMT.053 simplifié
- Documentation basique sur la structure des fichiers CAMT.053
- Des exemples de libellés avant/après transformation pour guider votre approche

## Exemples de transformation de libellés

Voici quelques exemples du type de transformations attendues :

| Libellé original | Libellé transformé |
|------------------|-------------------|
| PRLV SEPA MUTUELSANTE 552142259 | Prélèvement mutuelle santé - mensuel |
| CB CARREFOUR CITY PARIS 12/04 | Courses supermarché Carrefour City - 12 avril |
| VIR SEPA SALAIRE 042023 ENTREPRISE SA | Virement entrant - Salaire avril 2023 - Entreprise SA |
| FRAIS COMPTE COURANT | Frais bancaires - tenue de compte courant |
| CB SNCF INTERNET 15/04 74,00 EUR | Achat billet train SNCF - 15 avril - 74€ |
| DAB RETRAIT 23/04 200,00 EUR | Retrait distributeur - 23 avril - 200€ |

Bonne chance !
