# Guide simplifié du format CAMT.053

Le format CAMT.053 (Cash Management - Bank To Customer Statement) est un format XML standardisé utilisé par les banques pour fournir des relevés de compte électroniques à leurs clients.

## Structure principale

``` 
<Document>
  <BkToCstmrStmt>     <!-- Bank To Customer Statement -->
    <GrpHdr>          <!-- Group Header - informations générales sur le relevé -->
    </GrpHdr>
    <Stmt>            <!-- Statement - le relevé lui-même -->
      <Id>            <!-- Identifiant unique du relevé -->
      </Id>
      <Acct>          <!-- Account - informations sur le compte -->
      </Acct>
      <Bal>           <!-- Balance - solde du compte -->
      </Bal>
      <!-- Plusieurs éléments <Ntry> (Entry) - une entrée par transaction -->
      <Ntry>          <!-- Entry - une transaction -->
      </Ntry>
      <Ntry>
      </Ntry>
      <!-- etc. -->
    </Stmt>
  </BkToCstmrStmt>
</Document>
```

## Informations importantes pour chaque transaction (nœud `<Ntry>`)

Pour chaque transaction (élément `<Ntry>`), voici les nœuds importants à extraire :

```xml
<Ntry>
  <Amt Ccy="CHF">123.45</Amt>         <!-- Montant et devise -->
  <CdtDbtInd>CRDT</CdtDbtInd>          <!-- Type: CRDT=crédit, DBIT=débit -->
  <BookgDt>                            <!-- Date comptable -->
    <Dt>2025-03-02</Dt>
  </BookgDt>
  <ValDt>                              <!-- Date de valeur -->
    <Dt>2025-03-02</Dt>
  </ValDt>
  <BkTxCd>                             <!-- Code de transaction -->
    <Prtry>
      <Cd>PAYMENT</Cd>                 <!-- Type d'opération -->
    </Prtry>
  </BkTxCd>
  <NtryDtls>                           <!-- Détails de la transaction -->
    <TxDtls>                           <!-- Détails de transaction -->
      <Refs>
        <AcctSvcrRef>REF123</AcctSvcrRef>  <!-- Référence bancaire -->
      </Refs>
      <RltdPties>                      <!-- Parties liées -->
        <Dbtr>                         <!-- Débiteur (payeur) -->
          <Nm>NOM PAYEUR</Nm>          <!-- Nom du payeur -->
        </Dbtr>
        <!-- OU -->
        <Cdtr>                         <!-- Créditeur (bénéficiaire) -->
          <Nm>NOM BENEFICIAIRE</Nm>    <!-- Nom du bénéficiaire -->
        </Cdtr>
      </RltdPties>
      <RmtInf>                         <!-- Information de remise -->
        <Ustrd>LIBELLE</Ustrd>         <!-- Libellé non structuré - IMPORTANT pour notre test -->
      </RmtInf>
    </TxDtls>
  </NtryDtls>
</Ntry>
```

## Éléments clés à extraire pour le test

1. **Date de la transaction**: `<BookgDt><Dt>` ou `<ValDt><Dt>`
2. **Montant et devise**: `<Amt Ccy="CHF">123.45</Amt>`
3. **Type d'opération**: `<CdtDbtInd>` (CRDT pour crédit, DBIT pour débit)
4. **Référence**: `<AcctSvcrRef>` dans `<Refs>`
5. **Nom du tiers**: `<Nm>` dans `<Dbtr>` ou `<Cdtr>` (selon le type d'opération)
6. **Libellé**: `<Ustrd>` dans `<RmtInf>` - C'est ce libellé que nous voulons harmoniser

## Exemple d'extraction d'une transaction

Pour une entrée comme celle-ci:

```xml
<Ntry>
  <Amt Ccy="CHF">128.90</Amt>
  <CdtDbtInd>CRDT</CdtDbtInd>
  <BookgDt>
    <Dt>2025-03-02</Dt>
  </BookgDt>
  <ValDt>
    <Dt>2025-03-02</Dt>
  </ValDt>
  <NtryDtls>
    <TxDtls>
      <Refs>
        <AcctSvcrRef>REF2025030201</AcctSvcrRef>
      </Refs>
      <RltdPties>
        <Dbtr>
          <Nm>DURAND JEAN-PIERRE</Nm>
        </Dbtr>
      </RltdPties>
      <RmtInf>
        <Ustrd>REGLT FACT. 2542-A CAFE+CROISSANTS. MERCI JP</Ustrd>
      </RmtInf>
    </TxDtls>
  </NtryDtls>
</Ntry>
```

Les informations à extraire seraient:
- Date: 2025-03-02
- Montant: 128.90 CHF
- Type: Crédit (entrée d'argent)
- Référence: REF2025030201
- Tiers: DURAND JEAN-PIERRE
- Libellé à harmoniser: "REGLT FACT. 2542-A CAFE+CROISSANTS. MERCI JP"