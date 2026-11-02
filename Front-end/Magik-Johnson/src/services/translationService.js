// Mapping des langues
const LANGUAGES_MAP = {
  // Langues courantes
  English: 'Anglais',
  French: 'Français',
  German: 'Allemand',
  Spanish: 'Espagnol',
  Italian: 'Italien',
  Portuguese: 'Portugais',
  'Portuguese (Brazil)': 'Portugais (Brésil)',
  Russian: 'Russe',
  Japanese: 'Japonais',
  Korean: 'Coréen',
  'Chinese Simplified': 'Chinois simplifié',
  'Chinese Traditional': 'Chinois traditionnel',

  // Langues spéciales Magic
  'Ancient Greek': 'Grec ancien',
  Arabic: 'Arabe',
  Hebrew: 'Hébreu',
  Latin: 'Latin',
  Sanskrit: 'Sanskrit',
  Phyrexian: 'Phyrexian',
}

// Mapping des types de cartes
const TYPES_MAP = {
  // Types de base
  Artifact: 'Artefact',
  Battle: 'Bataille',
  Creature: 'Créature',
  Enchantment: 'Enchantement',
  Instant: 'Sort',
  Land: 'Terrain',
  Planeswalker: 'Planeswalker',
  Sorcery: 'Rituel',

  // Types spéciaux
  Conspiracy: 'Conspiration',
  Phenomenon: 'Phénomène',
  Plane: 'Plan',
  Scheme: 'Machination',
  Tribal: 'Tribal',
  Vanguard: 'Avant-garde',
  Emblem: 'Emblème',
  Token: 'Jeton',
  Card: 'Carte',
  Stickers: 'Autocollants',
  Attraction: 'Attraction',
  Dungeon: 'Donjon',
  Case: 'Cas',
  Role: 'Rôle',
}

// Mapping des sous-types de créatures (les plus courants)
const CREATURE_SUBTYPES_MAP = {
  // Races humanoïdes
  Human: 'Humain',
  Elf: 'Elfe',
  Dwarf: 'Nain',
  Goblin: 'Gobelin',
  Orc: 'Orque',
  Troll: 'Troll',
  Giant: 'Géant',
  Zombie: 'Zombie',
  Vampire: 'Vampire',
  Spirit: 'Esprit',
  Angel: 'Ange',
  Demon: 'Démon',
  Devil: 'Diable',
  Dragon: 'Dragon',
  Wizard: 'Sorcier',
  Warrior: 'Guerrier',
  Knight: 'Chevalier',
  Soldier: 'Soldat',
  Rogue: 'Voleur',
  Assassin: 'Assassin',
  Cleric: 'Clerc',
  Shaman: 'Chaman',
  Druid: 'Druide',
  Monk: 'Moine',
  Berserker: 'Berserker',
  Barbarian: 'Barbare',
  Scout: 'Éclaireur',
  Ranger: 'Rôdeur',
  Archer: 'Archer',
  Mercenary: 'Mercenaire',
  Pirate: 'Pirate',
  Rebel: 'Rebelle',
  Minion: 'Séide',
  Advisor: 'Conseiller',
  Noble: 'Noble',
  Peasant: 'Paysan',

  // Animaux
  Cat: 'Chat',
  Dog: 'Chien',
  Wolf: 'Loup',
  Bear: 'Ours',
  Elephant: 'Éléphant',
  Horse: 'Cheval',
  Bird: 'Oiseau',
  Eagle: 'Aigle',
  Hawk: 'Faucon',
  Snake: 'Serpent',
  Spider: 'Araignée',
  Rat: 'Rat',
  Beast: 'Bête',
  Boar: 'Sanglier',
  Ox: 'Bœuf',
  Goat: 'Chèvre',
  Sheep: 'Mouton',
  Fish: 'Poisson',
  Whale: 'Baleine',
  Octopus: 'Pieuvre',
  Crab: 'Crabe',
  Turtle: 'Tortue',
  Frog: 'Grenouille',
  Lizard: 'Lézard',
  Crocodile: 'Crocodile',
  Dinosaur: 'Dinosaure',
  Rhino: 'Rhinocéros',
  Hippo: 'Hippopotame',
  Ape: 'Singe',
  Minotaur: 'Minotaure',
  Centaur: 'Centaure',
  Sphinx: 'Sphinx',
  Griffin: 'Griffon',
  Phoenix: 'Phénix',
  Unicorn: 'Licorne',
  Pegasus: 'Pégase',
  Hydra: 'Hydre',

  // Créatures fantastiques
  Faerie: 'Fée',
  Elemental: 'Élémentaire',
  Golem: 'Golem',
  Construct: 'Artificiel',
  Homunculus: 'Homoncule',
  Gargoyle: 'Gargouille',
  Chimera: 'Chimère',
  Manticore: 'Manticore',
  Wurm: 'Guivre',
  Leviathan: 'Léviathan',
  Kraken: 'Kraken',
  Behemoth: 'Béhémoth',
  Avatar: 'Avatar',
  Incarnation: 'Incarnation',
  Horror: 'Horreur',
  Nightmare: 'Cauchemar',
  Shade: 'Ombre',
  Wraith: 'Spectre',
  Phantom: 'Fantôme',
  Banshee: 'Banshee',
  Lich: 'Liche',
  Skeleton: 'Squelette',
  Mummy: 'Momie',

  // Plantes et champignons
  Plant: 'Plante',
  Treefolk: 'Sylvin',
  Fungus: 'Champignon',
  Saproling: 'Saprobionte',

  // Créatures artificielles
  Thopter: 'Ornitère',
  Servo: 'Servo',
  Myr: 'Myr',
  Juggernaut: 'Djaggernaut',

  // Créatures planaires spéciales
  Eldrazi: 'Eldrazi',
  Phyrexian: 'Phyrexian',
  Sliver: 'Slivoïde',
  Ally: 'Allié',
  Zubera: 'Zubera',

  // Autres
  Mutant: 'Mutant',
  Alien: 'Alien',
  Weird: 'Anomalie',
  Shapeshifter: 'Métamorphe',
  Changeling: 'Changelin',
  Clone: 'Clone',
  Reflection: 'Reflet',
  Illusion: 'Illusion',
  Wall: 'Mur',
  Egg: 'Œuf',
  Slug: 'Limace',
  Ooze: 'Limon',
  Slime: 'Vase',
}

// Mapping des sous-types d'artefacts
const ARTIFACT_SUBTYPES_MAP = {
  Equipment: 'Équipement',
  Vehicle: 'Véhicule',
  Fortification: 'Fortification',
  Contraption: 'Engin',
  Food: 'Nourriture',
  Treasure: 'Trésor',
  Clue: 'Indice',
  Gold: 'Or',
  Blood: 'Sang',
  Powerstone: 'Pierre de puissance',
}

// Mapping des sous-types d'enchantements
const ENCHANTMENT_SUBTYPES_MAP = {
  Aura: 'Aura',
  Cartouche: 'Cartouche',
  Curse: 'Malédiction',
  Saga: 'Saga',
  Shrine: 'Sanctuaire',
  Class: 'Classe',
  Case: 'Cas',
  Role: 'Rôle',
  Background: 'Historique',
}

// Mapping des sous-types de terrains
const LAND_SUBTYPES_MAP = {
  Plains: 'Plaine',
  Island: 'Île',
  Swamp: 'Marais',
  Mountain: 'Montagne',
  Forest: 'Forêt',
  Wastes: 'Landes',
  Desert: 'Désert',
  Gate: 'Porte',
  Lair: 'Repaire',
  Locus: 'Locus',
  Mine: 'Mine',
  'Power-Plant': 'Centrale',
  Tower: 'Tour',
  "Urza's": 'Urza',
}

// Mapping des raretés
const RARITIES_MAP = {
  Common: 'Commune',
  Uncommon: 'Peu commune',
  Rare: 'Rare',
  Mythic: 'Mythique',
  'Mythic Rare': 'Rare mythique',
  Special: 'Spéciale',
  Bonus: 'Bonus',
  Token: 'Jeton',
  'Basic Land': 'Terrain de base',
}

// Mapping des couleurs
const COLORS_MAP = {
  White: 'Blanc',
  Blue: 'Bleu',
  Black: 'Noir',
  Red: 'Rouge',
  Green: 'Vert',
  Colorless: 'Incolore',
  Blanc: 'Blanc',
  Bleu: 'Bleu',
  Noir: 'Noir',
  Rouge: 'Rouge',
  Vert: 'Vert',
  Incolore: 'Incolore',
}

// Mapping des disponibilités
const AVAILABILITY_MAP = {
  'In Stock': 'En stock',
  'Out of Stock': 'En rupture de stock',
  'En stock': 'En stock',
  'En rupture de stock': 'En rupture de stock',
}

/**
 * Classe principale du service de traduction
 */
export class TranslationService {
  /**
   * Traduit le nom d'une langue
   * @param {string} language
   * @returns {string}
   */
  static translateLanguage(language) {
    // Si c'est déjà en français, le retourner tel quel
    if (Object.values(LANGUAGES_MAP).includes(language)) {
      return language
    }
    // Sinon traduire de l'anglais vers le français
    return LANGUAGES_MAP[language] || language
  }

  /**
   * Traduit le nom d'un type de carte
   * @param {string} type
   * @returns {string}
   */
  static translateType(type) {
    return TYPES_MAP[type] || type
  }

  /**
   * Traduit le nom d'un sous-type
   * @param {string} subtype
   * @returns {string}
   */
  static translateSubtype(subtype) {
    return (
      CREATURE_SUBTYPES_MAP[subtype] ||
      ARTIFACT_SUBTYPES_MAP[subtype] ||
      ENCHANTMENT_SUBTYPES_MAP[subtype] ||
      LAND_SUBTYPES_MAP[subtype] ||
      subtype
    )
  }

  /**
   * Traduit une rareté
   * @param {string} rarity
   * @returns {string}
   */
  static translateRarity(rarity) {
    if (!rarity) return rarity

    const frenchValues = Object.values(RARITIES_MAP)
    if (frenchValues.includes(rarity)) {
      return rarity
    }

    const normalizedRarity = rarity.charAt(0).toUpperCase() + rarity.slice(1).toLowerCase()

    return RARITIES_MAP[normalizedRarity] || RARITIES_MAP[rarity] || rarity
  }

  /**
   * Traduit une couleur
   * @param {string} color
   * @returns {string}
   */
  static translateColor(color) {
    // Si c'est déjà en français, le retourner tel quel
    if (Object.values(COLORS_MAP).includes(color)) {
      return color
    }
    // Sinon traduire de l'anglais vers le français
    return COLORS_MAP[color] || color
  }

  /**
   * Traduit une disponibilité
   * @param {string} availability
   * @returns {string}
   */
  static translateAvailability(availability) {
    return AVAILABILITY_MAP[availability] || availability
  }

  /**
   * Traduit une liste de langues
   * @param {Array} languages
   * @returns {Array}
   */
  static translateLanguages(languages) {
    return languages.map((lang) => this.translateLanguage(lang))
  }

  /**
   * Traduit une liste de types
   * @param {Array} types
   * @returns {Array}
   */
  static translateTypes(types) {
    return types.map((type) => this.translateType(type))
  }

  /**
   * Traduit une liste de sous-types
   * @param {Array} subtypes
   * @returns {Array}
   */
  static translateSubtypes(subtypes) {
    return subtypes.map((subtype) => this.translateSubtype(subtype))
  }

  /**
   * Traduit une liste de raretés
   * @param {Array} rarities
   * @returns {Array}
   */
  static translateRarities(rarities) {
    return rarities.map((rarity) => this.translateRarity(rarity))
  }

  /**
   * Traduit une liste de couleurs
   * @param {Array} colors
   * @returns {Array}
   */
  static translateColors(colors) {
    return colors.map((color) => this.translateColor(color))
  }

  /**
   * Fonction pour obtenir la traduction inverse (français vers anglais)
   * Utile pour envoyer les requêtes API dans la langue originale
   */
  static reverseTranslateLanguage(frenchLanguage) {
    const entry = Object.entries(LANGUAGES_MAP).find(
      ([english, french]) => french === frenchLanguage,
    )
    return entry ? entry[0] : frenchLanguage
  }

  static reverseTranslateType(frenchType) {
    const entry = Object.entries(TYPES_MAP).find(([english, french]) => french === frenchType)
    return entry ? entry[0] : frenchType
  }

  static reverseTranslateSubtype(frenchSubtype) {
    const allMaps = {
      ...CREATURE_SUBTYPES_MAP,
      ...ARTIFACT_SUBTYPES_MAP,
      ...ENCHANTMENT_SUBTYPES_MAP,
      ...LAND_SUBTYPES_MAP,
    }
    const entry = Object.entries(allMaps).find(([english, french]) => french === frenchSubtype)
    return entry ? entry[0] : frenchSubtype
  }

  static reverseTranslateRarity(frenchRarity) {
    const entry = Object.entries(RARITIES_MAP).find(([english, french]) => french === frenchRarity)
    return entry ? entry[0] : frenchRarity
  }

  static reverseTranslateColor(frenchColor) {
    // Le backend attend les couleurs en français minuscules
    const colorMapping = {
      Blanc: 'blanc',
      Bleu: 'bleu',
      Noir: 'noir',
      Rouge: 'rouge',
      Vert: 'vert',
      Incolore: 'incolore',
    }

    return colorMapping[frenchColor] || frenchColor.toLowerCase()
  }

  static reverseTranslateAvailability(frenchAvailability) {
    const entry = Object.entries(AVAILABILITY_MAP).find(
      ([english, french]) => french === frenchAvailability,
    )
    return entry ? entry[0] : frenchAvailability
  }

  /**
   * @param {Array} languageQuantities
   * @returns {string}
   */
  static determineBestLanguageFromInventory(languageQuantities) {
    if (!languageQuantities || languageQuantities.length === 0) {
      return 'Anglais'
    }

    const availableLanguages = languageQuantities
      .filter((lang) => {
        const totalStock = (lang.foilQuantity || 0) + (lang.nonFoilQuantity || 0)
        return totalStock > 0
      })
      .map((lang) => {
        const langName = lang.languageName || 'English'
        return this.translateLanguage(langName)
      })

    if (availableLanguages.length === 0) {
      return 'Anglais'
    }

    if (availableLanguages.includes('Français')) {
      return 'Français'
    } else if (availableLanguages.length > 0) {
      return availableLanguages[0]
    }

    return 'Anglais'
  }
}

// Export par défaut
export default TranslationService
