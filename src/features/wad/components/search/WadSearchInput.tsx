import clsx from 'clsx';
import { useCombobox } from 'downshift';
import React, { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { FaSearch } from 'react-icons/fa';
import { MdDataArray } from 'react-icons/md';
import { useDebounceCallback } from 'usehooks-ts';

import { getLeagueFileKindIcon, getLeagueFileKindIconColor, useSearchWad } from '../..';
import { Icon, Input, Popover } from '../../../../components';
import { appRoutes } from '../../../../lib/router';
import { composeUrlQuery } from '../../../../utils';

export type WadSearchInputProps = { wadId: string };

export const WadSearchInput: React.FC<WadSearchInputProps> = ({ wadId }) => {
  const [t] = useTranslation('common');

  const [searchQuery, setSearchQuery] = useState('');
  const handleSearchQueryChange = useDebounceCallback(setSearchQuery, 500);

  const searchWad = useSearchWad({ wadId, query: searchQuery });

  const { getInputProps, getMenuProps, isOpen, openMenu, closeMenu, getItemProps } = useCombobox({
    onInputValueChange: (changes) => {
      handleSearchQueryChange(changes.inputValue ?? '');
    },
    itemToString: (item) => item?.path ?? '',
    items: searchWad.data?.items ?? [],
  });

  return (
    <Popover.Root
      open={isOpen}
      onOpenChange={(open) => {
        open ? openMenu() : closeMenu();
      }}
    >
      <Popover.Anchor className="flex h-full">
        <Input
          {...getInputProps()}
          className="w-[500px]"
          placeholder="Search"
          right={<Icon size="md" icon={FaSearch} />}
        />
      </Popover.Anchor>
      <Popover.Content
        side="bottom"
        sideOffset={6}
        align="end"
        className="max-h-[600px] min-w-[500px] overflow-y-scroll"
      >
        <>
          {searchWad.data?.items.length === 0 && (
            <div className="flex flex-col items-center justify-center gap-1">
              <Icon size="xl" icon={MdDataArray} />
              <p className="select-none text-base text-gray-50">{t('noResults')}</p>
            </div>
          )}
          <ul {...getMenuProps({}, { suppressRefError: true })} className="flex flex-col">
            {searchWad.data?.items.map((item, index) => (
              <li
                key={index}
                {...getItemProps({
                  item,
                  index,
                })}
              >
                <a
                  className="flex flex-row items-center gap-2 rounded p-1 py-2 transition-colors duration-100 hover:cursor-pointer hover:bg-gray-600 hover:shadow-inner"
                  href={composeUrlQuery(appRoutes.mountedWads, { wadId, itemId: item.parentId })}
                >
                  <Icon
                    className={clsx(getLeagueFileKindIconColor(item.extensionKind))}
                    icon={getLeagueFileKindIcon(item.extensionKind)}
                  />
                  <p className="text-sm">{item.path}</p>
                </a>
              </li>
            ))}
          </ul>
        </>
      </Popover.Content>
    </Popover.Root>
  );
};
