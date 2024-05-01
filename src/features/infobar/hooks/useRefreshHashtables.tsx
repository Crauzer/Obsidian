import { useCallback, useRef, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { Id as ToastId, toast } from 'react-toastify';
import { v4 as uuidv4 } from 'uuid';

import { Link, Toast } from '../../../components';
import {
  apiErrorSchema,
  getApiErrorExtension,
  wadHashtablesMissingExtensionSchema,
} from '../../../types/error';
import { useLoadWadHashtables } from '../../hashtable';

export const useRefreshHashtables = () => {
  const [t] = useTranslation('common');

  const hashtablesLoadingToastId = useRef<ToastId>('');

  const [actionId] = useState(uuidv4());

  const loadHashtablesMutation = useLoadWadHashtables();

  const handleRefresh = useCallback(() => {
    hashtablesLoadingToastId.current = toast.info('Loading hashtables...', { autoClose: false });

    loadHashtablesMutation.mutate(
      { actionId },
      {
        onSuccess: () => {
          toast.update(hashtablesLoadingToastId.current, {
            type: 'success',
            render: 'Hashtables loaded!',
            autoClose: 2500,
          });
        },
        onError: (error) => {
          const apiError = apiErrorSchema.parse(error);
          const wadHashtablesMissingExtension = getApiErrorExtension(
            apiError,
            wadHashtablesMissingExtensionSchema,
          );

          if (wadHashtablesMissingExtension) {
            toast.update(hashtablesLoadingToastId.current, {
              type: 'warning',
              render: (
                <Toast.Warning
                  title={t('wadHashtablesMissing.title')}
                  message={<WadHashtablesMissingToastMessage />}
                />
              ),
              autoClose: false,
            });

            return;
          }

          toast.update(hashtablesLoadingToastId.current, {
            type: 'error',
            render: <Toast.Error title="Failed to load hashtables" message={error.message} />,
          });
        },
      },
    );
  }, [actionId, loadHashtablesMutation, t]);

  return { handleRefresh, loadHashtablesMutation };
};

const WadHashtablesMissingToastMessage = () => {
  const [t] = useTranslation();

  return (
    <div>
      <p>{t('wadHashtablesMissing.message.0')}</p>
      <p>
        {t('wadHashtablesMissing.message.1')}
        <Link href="https://github.com/Crauzer/Obsidian" target="_blank">
          {t('wadHashtablesMissing.message.2')}
        </Link>
      </p>
    </div>
  );
};
