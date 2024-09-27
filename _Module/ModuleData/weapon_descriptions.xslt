<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

	<xsl:output omit-xml-declaration="yes"/>

	<xsl:template match="@*|node()">
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='OneHandedSword']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_battania_noble_blade_1"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='TwoHandedSword']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_vlandian_blade_3"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='TwoHandedPolearm']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_spear_blade_13"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='OneHandedAxe']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_sickle_blade_1"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='TwoHandedAxe']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_axe_craft_38_head"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>

	<xsl:template match="WeaponDescription[@id='Javelin']/AvailablePieces/AvailablePiece[1]">
		<AvailablePiece id="tncs_spear_blade_27"/>
		<AvailablePiece id="tncs_spear_blade_15"/>
		<xsl:copy>
			<xsl:apply-templates select="@*|node()"/>
		</xsl:copy>
	</xsl:template>
	
</xsl:stylesheet>
